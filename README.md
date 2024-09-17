# hologram-lookingglass

Scenario/Use case:

The person is passing by - the Looking Glass System with the Leap Motion detects the person. The person is detected by means of their hands being found by the ultraleap hand-tracking camera module.

Cortana appears on the Looking Glass Screen and says ‘To start the interaction - please put on the glasses’ and she puts them on (Nataliya provided the model - it is the one to be used). 

We wait X sec for a person to put on the glasses. Those are BCI glasses - they enable brain sensing interaction. Cortana then says 'When the game is over, the king and the pawn go back into the same box', meaning the system (Cortana AND BCI) is initialized.

Then she asks: ‘Are you ready to start?’ and the user is supposed to do mental command YES on NO for two 10 second period. 

Then if the response is detected as YES - a pretty animation happens around her and she says ‘YES, MASTER CHIEF, WELCOME HOME’ and if the response is NO - she half disappears but then the question appears again ’Do you want to try again?'.

To see how this project is set up, as well as how to use other models or how Unity works, check the `Wiki` tab of this repo.

# How to set up application:

1. Make sure you have the Looking Glass plugged in to your computer, and the Looking Glass Bridge launched.

2. You must also make sure to have the Ultraleap hand tracking module connected to the computer, this will track your hands for the experience.

3. Make sure your Attentiv-U EEG is charged, and make sure that it is connected to the right socket (ws://backend.cortanahologram.com:3000).

4. Launch the Cortana.exe

5. Turn on BCI.

6. Put both hands over the Ultraleap hand tracker, do not obscure the cameras by being too close.

7. Follow the audio instructions from Cortana. Use your brain!


