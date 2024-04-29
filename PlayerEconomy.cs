using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure
{
    public class PlayerEconomy
    {
        public int Money { get; private set; }

        public void AddMoney(int amount)
        {
            Money += amount;
        }

        public void LoadMoney(int savedMoney)
        {
            Money = savedMoney;
        }

        public void SaveMoney()
        {
            // Assuming you have a method to save to a file or settings
            System.IO.File.WriteAllText("player_money.txt", Money.ToString());
        }
    }
}
