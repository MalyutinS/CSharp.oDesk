select j1.Skill as Skill1, j2.Skill as Skill2, count(j1.ODeskId) as Weight
from jobs j1 
inner join jobs j2 on j1.ODeskId = j2.ODeskId and j1.Skill > j2.Skill  
group by j1.Skill , j2.Skill
having count(j1.ODeskId) > 1
order by count(j1.ODeskId) desc

select cs1.Skill as Skill1, cs2.Skill as Skill2, count(cs1.ODeskId) as Weight
from Contractors_Skills cs1 
inner join Contractors_Skills cs2 on cs1.ODeskId = cs2.ODeskId and cs1.Skill > cs2.Skill  
group by cs1.Skill , cs2.Skill
having count(cs1.ODeskId) > 1
order by count(cs1.ODeskId) desc

select COUNT(distinct ODeskId) from jobs

select count(*) from jobs

select count(*) from Contractors_Skills

select count(*) from Contractors_Skills where Skill = 'wordpress'

select count(*) from Contractors



select count(distinct ODeskId) from contractors_skills where ODeskId not in (select distinct ODeskId from contractors)

