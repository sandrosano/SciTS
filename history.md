
# TODO
- general metrics csv /glances exception
- new dlts update
- outofRange fehler
- aggregated queries ueberpruefen
- VM as PrometheusQL



10.5. dlts bug gefixxt, postgres instal, ingestiontest

9.5. 
bugfixxing/ dlts bug report-analysis , update dlts, research promql

 
8.5 nach krankheit: heute bugfixxen aggquery polydim, generell testing
solving problems with outdated deprecated influxclient // restapi

2.mai
ich entscheide mich gegen  eine extra implementation des polydim, sondern dafuer die werte generell in einem array zu speichernund per interface eine get first value methode einzufiegen
gegen batchwrite overload, sonder fuer entscheidunge im code 
loest das postgres batch obj und redundanter code
3.5
 fuehre dict in dtlsdb ein um von dicht toarray direct werte uebergeben zu koenenn
 counter logik
# science

## ideen
- man koennte in den KIT logs erkennen, welche Query/ies besonders multidimensionale Daten abfragt, und einen benchmark querytyp dazubauen, da bisherige qUEIES NICHT besonders fuer MDs entwickelt wurden.