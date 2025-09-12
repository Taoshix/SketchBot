using System;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SketchBot.Utils
{
    public class XP
    {
        public static long returnXP(SocketMessage msg) /*We create a method that returns a integer and has SocketMessage as a parameter*/
        {
            Random rand = new Random(); /*Creates a new instance of random*/
            var msgCount = msg.Content.Length; /*Counts the total amount of characters in the message*/
            var xp = rand.Next(msgCount / 3);/*Calculates the xp by getting the total length of the message dividing it by 3 and then choosing a random integer from the amount this is flexible and you can change this to your own equation if you want*/
            return xp; /*Returns the xp*/
        }
        public static double caclulateNextLevel(long currentLevel)/*creates a new method that returns a integer and takes the current level as a parameter*/
        {
            var calc = Math.Pow(currentLevel + 1, 3)+25/currentLevel; /*Takes the current level adds a 1 to it and then multiples it by the power of 3, note this is also flexible and if you want your own equation for calculating the next level then go for it*/
            var calc2 = Convert.ToDouble(calc); /*Converts the calc variable to a long*/
            return calc2; /*Returns the required xp for level up*/
        }
    }
}
