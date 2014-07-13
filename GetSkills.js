// JavaScript source code

var skills = $(".oSkill > a");
var output = "";
for (i = 0; i < skills.length; i++) {
    output += (skills[i].title + "\t" + skills[i].href.replace("https://www.odesk.com/o/profiles/browse/skill/", "").replace("/", "") + "\n");
}
console.log(output);