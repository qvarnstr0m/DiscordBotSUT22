//Console application Discord bot with periodic webscraping functions
//Developed in .NET 6 with Discord.NET framework
//Martin Qvarnström SUT22 Campus Varberg, mqvarnstrom80@gmail.com, github: qvarnstr0m

//OnSiteLecture.cs class to specify a webscraped on site lecture object

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotSUT22.Modules
{
    internal class OnSiteLecture
    {
        //Properties that represent an on site lecture
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Building { get; set; }
        public string ClassRoom { get; set; }
        public string ClassAndSubject { get; set; }
        public string ExtraInfo { get; set; }

        //Constructor
        public OnSiteLecture(DateTime startTime, DateTime endTime, string building, string classroom, string classAndSubject, string extraInfo)
        {
            StartTime = startTime;
            EndTime = endTime;
            Building = building;
            ClassRoom = classroom;
            ClassAndSubject = classAndSubject;
            ExtraInfo = extraInfo;
        }

        //Override ToString method
        public override string ToString()
        {
            //return $"Starttime: {StartTime.ToString("HH:mm")}\nEndtime: {EndTime.ToString("HH:mm")}\nBuilding: {Building}\nClassroom: {ClassRoom}\nClass and subject: {ClassAndSubject}\nExtra info: {ExtraInfo}";

            if (ExtraInfo.Length > 3)
            {
                return $"God morgon SUT22! Dags att masa sig till {ClassRoom} i {Building}, lektionen börjar {StartTime.ToString("HH:mm")} " +
                    $"och slutar {EndTime.ToString("HH:mm")}. ({ExtraInfo}) Koda lugnt!";
            }
            else
            {
                return $"God morgon SUT22! Dags att masa sig till {ClassRoom} i {Building}, lektionen börjar {StartTime.ToString("HH:mm")} " +
                    $"och slutar {EndTime.ToString("HH:mm")}. Koda lugnt!";
            }
        }
    }

}
