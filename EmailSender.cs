#include <SPI.h>
#include <Ethernet.h>
#include <stdio.h>
#include <string.h>

// Nätverksinställningar
byte mac[] = { 0xA8, 0x61, 0x0A, 0xAF, 0x02, 0x1E };
IPAddress ip(192, 168, 0, 62);        
IPAddress subnet(255, 255, 254, 0);   
IPAddress gateway(192, 168, 0, 1);   
IPAddress dns(192, 168, 0, 1);    

char server[] = "mailgot.it.volvo.net";
int port = 25;

struct Person {
    const char* name;
    const char* email;
};

// Lista över personer som ska få mail vid tryckknapp av filterinlämning
Person recipients[] = {
    {"Safi", "safi.nazari@consultant.volvo.com"},
    {"Peter", "Peter.Maijgren@volvo.com"},
    {"Emil", "emil.ragland@volvo.com"},
    {"Samuel", "Samuel.meyer@consultant.volvo.com"}
};

// Antal mottagare (räknar automatiskt antal personer i listan)
const int antalpersoner = sizeof(recipients) / sizeof(recipients[0]);


EthernetClient client;
EthernetServer Server(80);                       //server port


// Definiera pinnen för reläet
const int relayPin = 7;  // Denna för Ingång från PLC
const int ledPin = 9; // Denna för Utgång för indikering
const int ellabknapp =6;


bool lastState = LOW;
bool lastState2 = LOW;






void setup()
{
  Serial.begin(9600);
  pinMode(relayPin, INPUT_PULLUP);
  pinMode(ellabknapp, INPUT_PULLUP);
  pinMode(ledPin, OUTPUT);
  Ethernet.begin(mac, ip, dns, gateway, subnet);
  delay(2000);
  Serial.println(F("Startup"));
  for (int i = 0; i < 5; i++) {
        digitalWrite(ledPin, HIGH);
        delay(400);
        digitalWrite(ledPin, LOW);
        delay(400);
  }
        digitalWrite(ledPin, HIGH);
        delay(2000);
        digitalWrite(ledPin, LOW);
  Serial.print(F("IP-adress: "));
  Serial.println(Ethernet.localIP());
  Serial.print(F("Subnät: "));
  Serial.println(Ethernet.subnetMask());
  Serial.print(F("Gateway: "));
  Serial.println(Ethernet.gatewayIP());
  Serial.print("DNS-server: ");
  Serial.println(Ethernet.dnsServerIP());


}


void loop() {
  
    bool currentState = !digitalRead(relayPin);
    bool currentState2 = !digitalRead(ellabknapp);

  //För tryckknapp vid vågrum

    if (currentState == HIGH && lastState == LOW) {
        Serial.print("Knapp 1");
          for (int i = 0; i < antalpersoner; i++)
        {
            sendEmail(recipients[i].name, recipients[i].email, "TEAMS FILTER REDO FÖR VÄGNING", "Nu finns det ett filter som behöver vägas", "Hej Bästa!" );
        }

    }

//För tryckknapp vid ellab
  
  if (currentState2 == HIGH && lastState == LOW) {
        Serial.print("Knapp 2");

           sendEmail("", "", "TESTSAMUEL RINGKLOCKA!", "Det finns en person som väntar inne på ellab", "Hej Allihopa");
    }
  
    lastState = currentState;
    delay(1000);
      }


//Skicka email funktionen

void sendEmail(const char* name, const char* email, const char* subject, const char* message, const char* hej) {

    Serial.println("Försöker skicka e-post till");


    Serial.println("Subject:");
    Serial.println(subject);
    Serial.println(email);
    Serial.println(hej);
    Serial.println(name);
    Serial.println(message);

    String line = String("Subject: (") + subject + ") ";
  

  //Styra Ledslingorna
    for (int i = 0; i < 10; i++) {
        digitalWrite(ledPin, HIGH);
        delay(200);
        digitalWrite(ledPin, LOW);
        delay(200);
  }

  //Ansluter till Clienten

    if (client.connect(server, port)) {
        client.println("EHLO mailgot.ut.volvo.com");
        delay(500);
        client.println("MAIL FROM: <volvopenta@volvo.com>");
        delay(500);
        client.println("RCPT TO: <group.id.a474985@volvo.com>");
        delay(500);
        client.println("DATA");
        delay(500);
         // Lägg till UTF-8 support
        client.println("Content-Type: text/plain; charset=UTF-8");
        client.println("Content-Transfer-Encoding: 8bit");
        delay(500);
        
        client.print("Subject: ");
        client.print(subject);
        client.print(" ");
        client.println(email);
      
        client.println();  

        client.println(hej);
        client.println(name);
        client.println(message);
        client.println("//Samuel ");
        client.println(".");
        client.println(".");
        client.println("QUIT");
      
        digitalWrite(ledPin, HIGH); 
        delay(3000); 
        digitalWrite(ledPin, LOW); 
        delay(300); 
      Serial.println("E-post skickad!");
      Serial.println(name);
      Serial.println(email);
    } else {
      
        Serial.println("Fel: Kunde inte ansluta till SMTP-servern.");

    }

    client.stop();
}

