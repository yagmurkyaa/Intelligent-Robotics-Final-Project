#include <Servo.h>
int Th1, Th2, tmp;
Servo  M1, M2;

void setup() 
{
  Serial.begin(9600);
  pinMode(13,OUTPUT); 
  digitalWrite(13,0);
  Th1 = 0;
  Th2 = 0;

  
  M1.attach(2);
  M1.write(90);
  M2.attach(3);
  M2.write(90);
  
}

void loop() 
{
  delay(200);

  if(Serial.available()>=2)
  {
    Th1 = Serial.read(); 
    Th2 = Serial.read();

     
    
    while(Serial.available()) tmp = Serial.read();   
    
    M1.write(Th1);
    M2.write(Th2);
    digitalWrite(13,1);
    delay(500);
    digitalWrite(13,0);
    delay(500);

    
   

    
   


  }
}
