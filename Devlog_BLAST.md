# Friday 9/16/2022

### Software - Anshal

Had my first look at the actual code and the actual setup, and popped in my code with separate arrays of position and velocity points. Also fixed some issues with names of the namespaces.

Experimented with feedback, it seems that the parameter for pidIdx needs to be 1 for the feedback to even work.
`_talon.GetSelectedSensorPosition(1)` Gives angle data
`_talon.GetSelectedSensorVelocity(1)` Gives velocity data

Debugging with print statements is done with
`Debug.Print();`

### Hardware - Zachary

```
For Zach to fill in.
```

# Sunday 9/18/2022

### Software - Anshal

Attempted to tune PIDs by changing constants to (1,0,0.2), however the motor's gearbox immediately broke, so tuning is delayed until that being fixed. 

For the rest of the workday, worked on making the brake work. It's very simply a digital pin connected on FY-5 (5.5)

Note to set up an enum for all the Ports so you can do FY.Pin5 for ease of use

How to set up a pin for use:
`OutputPort brakeSSR = new OutputPort(CTRE.HERO.IO.Port5.Pin5, false);`

Using a pin can be done with:
`brakeSSR.Write(true);`

Rememeber that the brake module is disabled when the pin is set to true.

Also remember to find out how to change the letter of a pin.

### Hardware - Zachary

```
For Zach to fill in.
```
