objCustAttribs = custAttributes.get $ 1 baseobject:false 
showproperties objCustAttribs 
for p in getPropNames objCustAttribs do
   format "% = %\n" p (getProperty $ p)