void setup(){}

void loop()
{
    char buf[64];

    if(RawHID.recv(buf, 10) > 0)  // Wait for a received raw hid report
    {
      digitalWriteFast(LED_BUILTIN, 1);
      delay(100);
      Serial.println(buf);            // and print it on Serial (actually a SEREMU interface in this case)
      digitalWriteFast(LED_BUILTIN, 0);
    }
}
