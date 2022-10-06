# Friday 9/16/2022

### Software - Anshal

Had my first look at the actual code and the actual setup, and popped in my code with separate arrays of position and velocity points. Also fixed some issues with names of the namespaces.

Experimented with feedback, it seems that the parameter for pidIdx needs to be 1 for the feedback to even work.
`_talon.GetSelectedSensorPosition(1)` Gives angle data
`_talon.GetSelectedSensorVelocity(1)` Gives velocity data

Debugging with print statements is done with
`Debug.Print();`

### Hardware - Zachary

Pulled apart motor & diagnosis of damage

- damage induced due to "floating input" issue & overnight event
  - motor was left on, attached to arm, spinning at 120RPM, likely for multiple days
- motor left on, causing aluminum toothed hex shaft to be chewed through & destroyed

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

Development of CREATE project

- printing and model development
  - created new base plate, adding fixtures for sling

Brake

- implemented brake physically
  - learned how to affix properly

# Monday 9/19/2022

### Software - Anshal

Had nothing to work on, as the motor was broken, so didn't come in today.

### Hardware - Zach

Met with Maziar, defined problem statement for CREATE, and did 3D print work 

# Tuesday 9/20/2022

### Software - Anshal

Had nothing to work on, as the motor was broken, so didn't come in today.

### Hardware - Zachary

Met with CREATE team and developed next steps for both HS outreach, as well as MAE150 project steps

- CREATE 
  
  - 2 phase design process
    1. Students in classroom build marshmallow bridges to learn about statics & design process.
    - sub goal of developing intuitive understanding of stress
    - we supplement with 3D CAD after project, giving a simulation of some bridge designs and what the stresses look like
      - IDEA: actually model some the students made?
    2. Design for launch adapted
    - create singular wall structure with least amount of filament possible
    - must be able to withstand 6G environment in centrifuge
      - teach CAD (TinkerCAD) & 3D printing, along with itterative design, kinematics, gravity, etc

- Next Steps: 
  
  1. follow up with Tali for cirriculum redesign so we can match & cater project
  2. Get access to TinkerCAD to learn how to better teach
  3. Get access to 3D printer specs & software

# Wednesday 9/28/2022

### Software - Anshal

Motor was fixed today, so I came in to tune pids. 

Defaults work badly, but definitely faster than the 1,0,0.02. We need to get controller to do solid PID tuning, so until then just messing around with Matlab tuning has been enlightening but as of yet, no results.

It seems that initializing the sensor causes wierd errors:
`_talon.ConfigSelectedFeedbackSensor(FeedbackDevice.CTRE_MagEncoder_Absolute, 1);`

Initially that parameter was 0, and when it was 0 it just straight up didn't stop. Changing it to 1 made it stop, and removing it has done the same.

That was removed, and since then its been working better.

Still need to fix that stupid CAN frame not received error. It barely actually sends frames, but I remember it not doing this a few weeks ago. Wierd.

Note to bring in custom arduino controller in, potentially uses D-input.

### Hardware - Zachary

```
For Zach to fill in
```

# Friday 9/30/2022

### Software - Anshal

Motor started out today fixed, ran some simple profiles, and it seems to be trying to stop again, which is good, but the errors are still coming in, which is bad

On a terrible note, I was a bit too late to hit the stop on one of the runs and heard something of a snap, and now the motor kinda just grinds.

On a separate note, it doesn't seem like the preprocessor directives are working with multiple profiles, need to test that again

Since motor was broken, wrote all of the PID tuner code, given that the arduino controller works. Note: it actually uses xinput, but theres a way to use xinput with the HERO, so that works.

Need to add an automatic rule that if the motor is decceling it just sets pid to 0,0,0,0

Also need to see if it is possible to increase the rate at which frames are given from the FX, it seems we're still getting garbage response.

Macro for FY, AUSX, etc. didn't work.

### Hardware - Zachary

```
For Zachary to fill out.s
```

# Monday 10/3/2022

### Software - Anshal

Started with fixing up some modes on PID Tuner, current pids are far too agressive with no load, but the controller broke midway

Swapped over to working on motion profiling, since that was the main objective of today, figured out how to turn on Coast mode in code, other than that, little progress. Emailed support about the memory issue, since the 3001 point profile is too big, and our actual profile is 60k points, so thats an important issue that needs to be fixed.

# Wednesday 10/5/2022

Got responses to my emails, and learned how to:

- Interpolate (WIP)

- Increase status frame windows (WIP)

- [ADD]
