using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GPUInstancing : MonoBehaviour
{
	[SerializeField] [Range(1, 1020)] private int instanceCount;
	[SerializeField] private float force;
	[SerializeField] private Vector3 gravity;
	[SerializeField] [Range(0, 1000)] private float upForce = 0.4f;
	[SerializeField] [Range(0.1f, 10)] private float friction;
	[SerializeField] private float radius;
	[SerializeField] private Vector2 positionRange;
	[SerializeField] private Vector2 scaleRange;
	[SerializeField] private Mesh mesh;
	[SerializeField] private Material material;
	[SerializeField] private Transform head;
	[SerializeField] private LayerMask groundMask;
	[Tooltip("Every leaf above this rectangle's Y, and not inside the rectangle's XZ values will not perform a " +
			 "raycast.")]
	[SerializeField]
	private Shape distributionShape = Shape.Rectangle;

	private enum Shape
	{
		Rectangle,
		Ellipse
	}

	private Vector3[][] velocities;
	private Matrix4x4[][] matrices;
	private float[][] groundHeights;
	private bool[][] didReport;
	private bool[][] isOverWater;
	private float[][] groundedTimers;
	private float[][] timerLastHit;
	private float[][] lastInGroundTimer;
	private VelocityUtil velocityUtil;

	private MaterialPropertyBlock mpb;

	public static int _leafCountOnMainGround = 0;
	public static int _startNumOfLeaves = 0;
	private Vector3 _minLeafLocalPos;

	private float _startCd = 0.5f;

	private JobHandle _rayCastHandler;
	private bool _raycastJobStarted;

	public static void RestartLeaves()
	{
		_leafCountOnMainGround = 0;
		_startNumOfLeaves = 0;
	}

	private void OnEnable()
	{
		_startNumOfLeaves += instanceCount;
		_leafCountOnMainGround += instanceCount;
		mpb = new MaterialPropertyBlock();
		velocityUtil = new VelocityUtil(head);
		velocities = new Vector3[instanceCount / 1023 + 1][];
		matrices = new Matrix4x4[instanceCount / 1023 + 1][];
		groundHeights = new float[instanceCount / 1023 + 1][];
		didReport = new bool[instanceCount / 1023 + 1][];
		isOverWater = new bool[instanceCount / 1023 + 1][];
		groundedTimers = new float[instanceCount / 1023 + 1][];
		timerLastHit = new float[instanceCount / 1023 + 1][];
		lastInGroundTimer = new float[instanceCount / 1023 + 1][];
		for (int i = 0; i < instanceCount / 1023 + 1; i++)
		{
			int iterations;
			if (i == (instanceCount / 1023 + 1) - 1)
			{
				iterations = instanceCount % 1023;
				matrices[i] = new Matrix4x4[iterations];
				velocities[i] = new Vector3[iterations];
				groundHeights[i] = new float[iterations];
				didReport[i] = new bool[iterations];
				isOverWater[i] = new bool[iterations];
				groundedTimers[i] = new float[iterations];
				timerLastHit[i] = new float[iterations];
				lastInGroundTimer[i] = new float[iterations];
			}
			else
			{
				iterations = 1023;
				matrices[i] = new Matrix4x4[1023];
				velocities[i] = new Vector3[1023];
				groundHeights[i] = new float[1023];
				didReport[i] = new bool[1023];
				isOverWater[i] = new bool[1023];
				groundedTimers[i] = new float[1023];
				timerLastHit[i] = new float[1023];
				lastInGroundTimer[i] = new float[1023];
			}
			for (int j = 0; j < iterations; j++)
			{
				var newPos = new Vector3(transform.position.x + Random.Range(-positionRange.x, positionRange.x),
					transform.position.y,
					transform.position.z + Random.Range(-positionRange.y, positionRange.y));
				if (distributionShape == Shape.Ellipse)
				{
					var angleX = Random.Range(0, 2f * Mathf.PI);
					var angleY = Random.Range(0, 2f * Mathf.PI);
					var radiusX = Random.Range(-positionRange.x, positionRange.x);
					var radiusY = Random.Range(-positionRange.y, positionRange.y);
					var xValue = Mathf.Cos(angleX) * radiusX;
					var yValue = Mathf.Sin(angleY) * radiusY;
					newPos = new Vector3(transform.position.x + xValue,
						transform.position.y,
						transform.position.z + yValue);
				}
				var randomRotate = new Vector3(0, Random.Range(-360, 360), 0);
				var scale = Random.Range(scaleRange.x, scaleRange.y);
				var randomScale = Vector3.one * scale;
				matrices[i][j] = Matrix4x4.TRS(newPos, Quaternion.Euler(randomRotate), randomScale);
				velocities[i][j] = Vector3.zero;
				groundHeights[i][j] = 0f;
				didReport[i][j] = false;
				isOverWater[i][j] = false;
				groundedTimers[i][j] = 0f;
				timerLastHit[i][j] = 0f;
				lastInGroundTimer[i][j] = 0f;
			}
		}
	}

	private void Update()
	{
		if (_startCd > 0)
		{
			_startCd -= Time.deltaTime;
			return;
		}
		velocityUtil.Update();
		CalculateMatricesJobs();
		Draw();
	}

	private void CalculateMatricesJobs()
	{
		for (int i = 0; i < instanceCount / 1023 + 1; i++)
		{
			var matricesChunk = this.matrices[i];
			var velocitiesChunk = this.velocities[i];
			var didReportChunk = this.didReport[i];
			var isOverWaterChunk = this.isOverWater[i];
			var groundedTimersChunk = this.groundedTimers[i];
			var timerLastHitChunk = this.timerLastHit[i];
			var lastInGroundTimerChunk = this.lastInGroundTimer[i];
			var matrices = new NativeArray<Matrix4x4>(matricesChunk, Allocator.TempJob);
			var velocities = new NativeArray<Vector3>(velocitiesChunk, Allocator.TempJob);
			var didReport = new NativeArray<Boolean>(didReportChunk, Allocator.TempJob);
			var isOverWater = new NativeArray<bool>(isOverWaterChunk, Allocator.TempJob);
			var groundedTimers = new NativeArray<float>(groundedTimersChunk, Allocator.TempJob);
			var timerLastHit = new NativeArray<float>(timerLastHitChunk, Allocator.TempJob);
			var lastInGroundTimer = new NativeArray<float>(lastInGroundTimerChunk, Allocator.TempJob);
			NativeArray<float> groundHeightPos;
			if (!_raycastJobStarted) // raycasts not started
			{
				StartRaycastJob(matricesChunk);
			}
			if (_raycastJobStarted && _rayCastHandler.IsCompleted) // raycasts finished
			{
				groundHeightPos = GetRaycastResults(matricesChunk, isOverWater);
				_raycastJobStarted = false;
			}
			else
			{
				groundHeightPos = new NativeArray<float>(groundHeights[i], Allocator.TempJob);
			}
			var job = new PhysicsJob()
			{
				activeBlower = Input.GetKey(KeyCode.Space) && !GameManager.Shared.RecentlySpawned(),
				deltaTime = Mathf.Min(Time.deltaTime, 0.1f),
				unscaledDeltaTime = Time.unscaledDeltaTime,
				force = force,
				friction = friction,
				groundHeights = groundHeightPos,
				headPosition = head.position,
				radius = radius,
				speed = velocityUtil.speed,
				upForce = upForce,
				baseSeed = i + 1,
				velocities = velocities,
				matrices = matrices,
				gravity = gravity,
				didReport = didReport,
				groundedTimers = groundedTimers,
				headForward = head.forward,
				timerLastHit = timerLastHit,
				lastInGroundTimer = lastInGroundTimer,
				isOverWater = isOverWater,
				isStageOver = GameManager.Shared.isStageOver,
				stageBounds = GameManager.Shared.GetIslandBounds()
			}.Schedule(matrices.Length, 64);
			job.Complete();
			for (int j = 0; j < velocities.Length; j++)
			{
				this.velocities[i][j] = velocities[j];
			}
			NativeArray<Matrix4x4>.Copy(matrices, this.matrices[i]);
			NativeArray<Vector3>.Copy(velocities, this.velocities[i]);
			NativeArray<float>.Copy(groundHeightPos, this.groundHeights[i]);
			NativeArray<float>.Copy(groundedTimers, this.groundedTimers[i]);
			NativeArray<float>.Copy(timerLastHit, this.timerLastHit[i]);
			NativeArray<float>.Copy(lastInGroundTimer, this.lastInGroundTimer[i]);
			int countLeaves = 0;
			var jobResult = didReport.ToArray();
			for (int j = 0; j < didReportChunk.Length; j++)
			{
				if (jobResult[j] && didReportChunk[j] == false)
				{
					countLeaves++;
				}
			}
			// parallel for
			_leafCountOnMainGround -= countLeaves;
			NativeArray<Boolean>.Copy(didReport, this.didReport[i]);
			NativeArray<bool>.Copy(isOverWater, this.isOverWater[i]);
			matrices.Dispose();
			velocities.Dispose();
			groundHeightPos.Dispose();
			didReport.Dispose();
			groundedTimers.Dispose();
			timerLastHit.Dispose();
			lastInGroundTimer.Dispose();
			isOverWater.Dispose();
		}
	}

	private void OnDestroy()
	{
		if (_raycastCommands.IsCreated)
		{
			_raycastCommands.Dispose();
		}
		if (_raycastResults.IsCreated)
		{
			_raycastResults.Dispose();
		}
	}

	private NativeArray<RaycastCommand> _raycastCommands;
	private NativeArray<RaycastHit> _raycastResults;

	private void StartRaycastJob(Matrix4x4[] leafPositions)
	{
		if (leafPositions.Length == 0)
		{
			print("Wtf?");
			return;
		}
		_raycastJobStarted = true;
		_raycastCommands = new NativeArray<RaycastCommand>(leafPositions.Length, Allocator.Persistent);
		_raycastResults = new NativeArray<RaycastHit>(leafPositions.Length, Allocator.Persistent);
		for (int i = 0; i < leafPositions.Length; i++)
		{
			_raycastCommands[i] = new RaycastCommand(
				leafPositions[i].GetPosition() + Vector3.up * 5f, Vector3.down,
				6f, groundMask);
		}
		_rayCastHandler = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastResults, leafPositions.Length);
	}

	private NativeArray<float> GetRaycastResults(Matrix4x4[] leafPositions, NativeArray<bool> isOverWaterChunk)
	{
		_rayCastHandler.Complete();
		var heights = new NativeArray<float>(leafPositions.Length, Allocator.TempJob);
		for (int i = 0; i < _raycastResults.Length; i++)
		{
			var hit = _raycastResults[i];
			if (hit.collider)
			{
				heights[i] = hit.point.y;
				isOverWaterChunk[i] = hit.collider.gameObject.CompareTag("downBlock");
			}
			else
			{
				heights[i] = -100f;
			}
		}
		_raycastCommands.Dispose();
		_raycastResults.Dispose();
		return heights;
	}

	private void Draw()
	{
		foreach (Matrix4x4[] batch in matrices)
		{
			Graphics.DrawMeshInstanced(mesh, 0, material, batch, batch.Length, mpb,
				ShadowCastingMode.On);
		}
	}

	[BurstCompile]
	public struct PhysicsJob : IJobParallelFor
	{
		[ReadOnly] public float force;
		[ReadOnly] public float upForce;
		[ReadOnly] public float radius;
		[ReadOnly] public float speed;
		[ReadOnly] public float deltaTime;
		[ReadOnly] public float unscaledDeltaTime;
		[ReadOnly] public float friction;

		[ReadOnly] public Vector3 headPosition;
		[ReadOnly] public Vector3 gravity;

		[ReadOnly] public int baseSeed;

		public NativeArray<Matrix4x4> matrices;
		public NativeArray<Vector3> velocities;
		public NativeArray<float> groundHeights;
		public NativeArray<Boolean> didReport;
		public NativeArray<float> groundedTimers;
		public NativeArray<float> timerLastHit;
		public NativeArray<float> lastInGroundTimer;
		public NativeArray<bool> isOverWater;

		[ReadOnly] public bool activeBlower;
		[ReadOnly] public Vector3 headForward;
		[ReadOnly] public bool isStageOver;
		[ReadOnly] public Bounds stageBounds;

		public PhysicsJob(bool activeBlower, float force, float upForce, float radius,
						  float speed, float deltaTime, float unscaledDeltaTime, float friction,
						  Vector3 headPosition, Vector3 gravity, int baseSeed,
						  NativeArray<Matrix4x4> matrices,
						  NativeArray<Vector3> velocities,
						  NativeArray<float> groundHeights,
						  NativeArray<Boolean> didReport,
						  NativeArray<float> groundedTimers,
						  Vector3 headForward,
						  NativeArray<float> timerLastHit,
						  NativeArray<float> lastInGroundTimer,
						  NativeArray<bool> isOverWater,
						  bool isStageOver,
						  Bounds stageBounds)
		{
			this.activeBlower = activeBlower;
			this.force = force;
			this.upForce = upForce;
			this.radius = radius;
			this.speed = speed;
			this.deltaTime = deltaTime;
			this.unscaledDeltaTime = unscaledDeltaTime;
			this.friction = friction;
			this.headPosition = headPosition;
			this.gravity = gravity;
			this.baseSeed = baseSeed;
			this.matrices = matrices;
			this.velocities = velocities;
			this.groundHeights = groundHeights;
			this.didReport = didReport;
			this.groundedTimers = groundedTimers;
			this.headForward = headForward;
			this.timerLastHit = timerLastHit;
			this.lastInGroundTimer = lastInGroundTimer;
			this.isOverWater = isOverWater;
			this.isStageOver = isStageOver;
			this.stageBounds = stageBounds;
		}

		public void Execute(int index)
		{
			var seed = baseSeed + index;
			var rnd = new Unity.Mathematics.Random((uint)seed);
			matrices[index].Decompose(out Vector3 pos, out Quaternion rot, out Vector3 scale);
			if (isStageOver)
			{
				if (isOverWater[index]) return;
				EndOfStageBlowEffect(index, pos, rot, scale, rnd);
				return;
			}
			if (pos.y > groundHeights[index] + 0.2f)
			{
				velocities[index] -= gravity * deltaTime;
			}
			var dist = Vector3.Distance(headPosition, pos);
			var dirToPos = (headPosition - pos).normalized; // Direction vector from headPosition to pos
			var angleToPos = Vector3.Angle(new Vector3(headForward.x, 0, headForward.z),
				new Vector3(dirToPos.x, 0, dirToPos.z)); // Angle between headForward and dirToPos
			bool shouldAffectLeaf = dist < radius && activeBlower && angleToPos <= 45;
			if (shouldAffectLeaf)
			{
				var t = 1 - dist / radius;
				var dir = headPosition - pos;
				dir.y -= upForce;
				var distForce = (1 / (dir.magnitude)) * 50f;
				velocities[index] += dir * (deltaTime * t * distForce * force);
				groundedTimers[index] = 0.1f;
				timerLastHit[index] = 1f;
			}
			// ROTATION!
			quaternion q;
			if (velocities[index].y < 0) //pos.y - groundHeights[index] > 2)
			{
				q = quaternion.Euler(rnd.NextFloat3(180, 360));
				rot = math.slerp(rot, q, 0.05f);
			}
			else
			{
				//q = quaternion.identity;
				q = new quaternion(0f, rot.y, 0f, 1f);
				rot = math.slerp(rot, q, 0.05f);
			}
			// ROTATION END!
			// ground timers is a timer for last hit by the player
			// velocities[index].y < 0 means he's going up
			if (pos.y <= groundHeights[index] + 0.2f)
			{
				lastInGroundTimer[index] += deltaTime;
			}
			else
			{
				lastInGroundTimer[index] = 0;
			}
			if (pos.y <= groundHeights[index] + 0.1f &&
				groundedTimers[index] < 0) // if it will fall down is the third condition
			{
				if (!shouldAffectLeaf)
				{
					if (lastInGroundTimer[index] < 0.2f && !isOverWater[index])
					{
						velocities[index] = new Vector3(velocities[index].x, 0, velocities[index].z);
					}
					else if (!shouldAffectLeaf)
					{
						velocities[index] = Vector3.zero;
					}
				}
				pos.y = groundHeights[index] + 0.1f;
			}
			groundedTimers[index] -= deltaTime;
			timerLastHit[index] -= deltaTime;
			pos -= velocities[index] * deltaTime;
			matrices[index] = float4x4.TRS(pos, rot, scale);
			UpdateLeafCountIfNeeded(index, pos);
		}

		private void EndOfStageBlowEffect(int index, Vector3 pos, Quaternion rot, Vector3 scale,
										  Unity.Mathematics.Random rnd)
		{
			if (timerLastHit[index] > 0)
			{
				if (pos.y > groundHeights[index] + 0.2f)
				{
					velocities[index] -= gravity * unscaledDeltaTime;
				}
				timerLastHit[index] -= unscaledDeltaTime;
				pos -= velocities[index] * unscaledDeltaTime;
				matrices[index] = float4x4.TRS(pos, rot, scale);
				return;
			}
			var dir = headPosition - pos;
			dir.y -= upForce;
			velocities[index] += dir * (unscaledDeltaTime) * 500f;
			timerLastHit[index] = 1000f;
			pos -= velocities[index] * unscaledDeltaTime;
			matrices[index] = float4x4.TRS(pos, rot, scale);
		}

		private void UpdateLeafCountIfNeeded(int index, Vector3 pos)
		{
			if (!didReport[index] && !stageBounds.Contains(pos))
			{
				didReport[index] = true;
			}
		}
	}
}