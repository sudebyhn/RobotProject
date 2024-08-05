 # A robot project that detects a black spot and shoots. 

This project involves creating a robot that detects a black spot and shoots. It utilizes the following components:

- **Arduino IDE**: Used to program the Arduino board to interact with sensors and control the servo motor.
- **Arduino Nano**: Microcontroller board used for controlling the robot's functions.
- **Breadboard**: Used for prototyping and connecting electronic components.
- **Pan Tilt Base**: Mechanism used to adjust the orientation of the sensor and shooting mechanism.
- **Servo Motor**: Used to move the shooting mechanism.
- **Various Cables**: Used for connecting components and creating electrical connections.

## Arduino Sketch

The Arduino sketch is responsible for reading sensor data and controlling the servo motor. It detects the black spot and triggers the shooting mechanism when necessary.

```Arduino
#include <Servo.h>
#define pan_err 0
#define tilt_err 12
Servo M1, M2; 


int laser;
int Th1 =0;
int Th2 =0;

void setup() {
  Serial.begin(9600);
  pinMode(13, OUTPUT);
  digitalWrite(13, LOW);


  M1.attach(3);//pan
  M2.attach(4);//tilt
    M1.write(90+ pan_err);
    M2.write(90 +tilt_err);
}

void loop() {
  delay(200);

  if (Serial.available() >= 3) {
    Th1 = Serial.read();
    Th2 = Serial.read();
    laser = Serial.read();

    // Remove any extra wrong readings
    while (Serial.available())
      Serial.read();    

    M1.write(Th1 + pan_err);
    M2.write(Th2 + tilt_err);

 if (laser == 1)
    digitalWrite(13, HIGH); // Turn the laser on
else 
    digitalWrite(13, LOW);  // Turn the laser off

}
}
```
![Robot Interface]("C:\Users\casper\Desktop\images\Robot_Interface.jpg")

