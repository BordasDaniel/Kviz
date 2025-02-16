using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kviz
{
    public class Quiz
    {
        public static int lastQuestionsIndex = 0;
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Answer3 { get; set; }
        public bool Answer1Ans { get; set; }
        public bool Answer2Ans { get; set; }
        public bool Answer3Ans { get; set; }

        public Quiz() {}

        public Quiz(string question, string answer1, string answer2, string answer3, bool answer1Ans, bool answer2Ans, bool answer3Ans)
        {
            Id = ++lastQuestionsIndex;
            Question = question;
            Answer1 = answer1;
            Answer2 = answer2;
            Answer3 = answer3;
            Answer1Ans = answer1Ans;
            Answer2Ans = answer2Ans;
            Answer3Ans = answer3Ans;
        }

        public Quiz(string sor)
        {
            string[] adatok = sor.Split(';');
            Id = ++lastQuestionsIndex;
            Question = adatok[0];
            Answer1 = adatok[1];
            Answer2 = adatok[2];
            Answer3 = adatok[3];
            Answer1Ans = bool.Parse(adatok[4]);
            Answer2Ans = bool.Parse(adatok[5]);
            Answer3Ans = bool.Parse(adatok[6]);
        }
        


    }
}
