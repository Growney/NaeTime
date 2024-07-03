#define DEBUG 1

#if DEBUG == 1
  #define debugprintln(x) Serial.println(x) 
  #define debugprint(x) Serial.print(x) 
#else
  #define debugprintln(x) 
  #define debugprint(x)
#endif