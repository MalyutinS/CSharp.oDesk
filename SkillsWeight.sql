select j1.Skill as Skill1, j2.Skill as Skill2, count(j1.ODeskId) as Weight
from jobs j1 
inner join jobs j2 on j1.ODeskId = j2.ODeskId and j1.Skill > j2.Skill  
group by j1.Skill , j2.Skill
having count(j1.ODeskId) > 1
order by count(j1.ODeskId) desc

