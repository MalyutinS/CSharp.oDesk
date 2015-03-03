select j1.Skill as Skill1, j2.Skill as Skill2, count(j1.ODeskId) as Weight
from jobs j1 
inner join jobs j2 on j1.ODeskId = j2.ODeskId and j1.Skill <> j2.Skill  
group by j1.Skill , j2.Skill
having count(j1.ODeskId) > 0
order by count(j1.ODeskId) desc

select cs1.Skill as Skill1, 
       cs2.Skill as Skill2, 
       count(cs1.ODeskId) as Weight,
	   avg(c.TotalHours) as hours,
	   avg(c.BillRate) as rate
from Contractors_Skills cs1 
inner join Contractors_Skills cs2 on cs1.ODeskId = cs2.ODeskId and cs1.Skill <> cs2.Skill  
join Contractors c on c.ODeskId = cs1.ODeskId
group by cs1.Skill , cs2.Skill
having count(cs1.ODeskId) > 0

select cs.Skill , 
       count(cs.ODeskId) as cnt,
	   avg(c.TotalHours) as hours,
	   avg(c.BillRate) as rate
from Contractors_Skills cs
join Contractors c on c.ODeskId = cs.ODeskId
group by cs.Skill 

select  cs1.Skill as Skill1, 
		cs2.Skill as Skill2, 
		count(cs1.ODeskId) as Weight,
		avg(c.TotalHours) as hours,
		avg(c.BillRate) as rate,
		r.name
from Contractors_Skills cs1 
inner join Contractors_Skills cs2 on cs1.ODeskId = cs2.ODeskId and cs1.Skill > cs2.Skill  
join Contractors c on c.ODeskId = cs1.ODeskId
join ranges r on c.TotalHours >= r.Min AND c.TotalHours < r.Max
--where cs1.Skill = 'php' AND cs2.Skill = 'c#'
group by cs1.Skill , cs2.Skill, r.name
having count(cs1.ODeskId) > 1
order by avg(c.BillRate) desc



select avg(c.TotalHours) as hours, 
avg(c.BillRate) as rate, 
count(c.OdeskID) as cnt, 
r.name, 
cs.Skill from Contractors c
join Contractors_Skills cs on c.ODeskId = cs.ODeskId
join ranges r on c.TotalHours >= r.Min AND c.TotalHours < r.Max
--where cs.Skill = 'c#'
group by cs.Skill, r.name
--having count(c.OdeskID) > 10
--order by rate desc


select top 100 avg(c.TotalHours) as hours, 
avg(c.BillRate) as rate, 
count(c.OdeskID) as cnt, 
r.name, 
cs.Skill from Contractors c
join Contractors_Skills cs on c.ODeskId = cs.ODeskId
join ranges r on c.TotalHours >= r.Min AND c.TotalHours < r.Max
where cs.Skill = 'php'
group by cs.Skill, r.name
--having count(c.OdeskID) > 10
order by rate desc



select COUNT(distinct ODeskId) from jobs

select count(*) from jobs

select count(*) from Contractors_Skills

select count(*) from Contractors_Skills where Skill = 'wordpress'

insert into ranges (name,min,max) values ('1-10',1,10)
insert into ranges (name,min,max) values ('10-100',10,100)
insert into ranges (name,min,max) values ('100-500',100,500)
insert into ranges (name,min,max) values ('500+',500,1000000)


select count(distinct ODeskId) from contractors_skills where ODeskId not in (select distinct ODeskId from contractors)

with t as (select j1.Skill as Skill1, j2.Skill as Skill2, count(j1.ODeskId) as Weight
from jobs j1 
inner join jobs j2 on j1.ODeskId = j2.ODeskId and j1.Skill <> j2.Skill  
group by j1.Skill , j2.Skill
having count(j1.ODeskId) > 1)
select t1.Skill2, sum(t1.Weight) 
from t as t1
where t1.Skill1 in    ('c#','jquery','javascript','html','html5','css','css3','website-development','bootstrap','ajax','mysql','asp','sql','.net-framework','asp.net-mvc')
 and t1.Skill2 not in ('c#','jquery','javascript','html','html5','css','css3','website-development','bootstrap','ajax','mysql','asp','sql','.net-framework','asp.net-mvc')
group by t1.Skill2 
order by sum(t1.Weight) desc