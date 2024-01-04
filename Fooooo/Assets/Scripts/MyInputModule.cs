using UnityEngine.EventSystems;

public class MyInputModule : StandaloneInputModule
{
	public override void Process()
	{
		bool usedEvent = SendUpdateEventToSelectedObject();

		if (eventSystem.sendNavigationEvents)
		{
			if (!usedEvent)
				usedEvent |= SendMoveEventToSelectedObject();

			if (!usedEvent)
				SendSubmitEventToSelectedObject();
		}
	}
}
