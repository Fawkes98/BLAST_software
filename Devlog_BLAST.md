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

### Tuesday 9/20/2022

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


