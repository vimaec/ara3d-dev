weaponDataCA= attributes weaponData
(
parameters main rollout:params
(
hitPoints type:#float ui:hits default:10
cost type:#float ui:cost default:100
sound type:#string default:"boom"
on Cost set val do ( print "cost changed" )
)
 

rollout params "Weapon Parameters"
(
spinner hits "Hit Points" type: #float
spinner cost "Cost" type: #float
dropdownlist sound_dd "Sound" items:# ("boom","sparkle","zap","fizzle")
 
on sound_dd selected i do sound = sound_dd.items [i]
)
)