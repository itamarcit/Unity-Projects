[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-24ddc0f5d75046c5622901739e7c5dd533143b0c8e959d652212380cedb1ea36.svg)](https://classroom.github.com/a/ikqyXK2I)
[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-718a45dd9cf7e7f842a935f5ebbe5719a5e09af4491e668f4dbf3b35d5cca122.svg)](https://classroom.github.com/online_ide?assignment_repo_id=11064832&assignment_repo_type=AssignmentRepo)
# 🫠✨🦔 DissolveEffect Shader Exercise  🦔✨🫠
## Student Names 🤓
Dan Oren, Itamar Citrin

## Explain your design. What did you try to achieve? 💪

We wanted to create a dissolve effect that stretches the creature upwards and downwards,
while it starts to disappear randomly. To achieve that, we used Perlin Noise from the Jimmy's noise
package, and we then moved the vertices positions using cosine of time.  

## What would you improve given more time? 🥵

- [ ] If we were given more time, we would have created an effect specifically designed for the spikes, so that before the
creature dies, it would fire all its spikes in their pointing direction, and only then the dissolve effect would occur.
