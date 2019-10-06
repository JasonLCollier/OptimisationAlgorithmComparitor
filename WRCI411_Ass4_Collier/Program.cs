using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRCI411_Ass4_Collier
{
    class Program
    {
        static void Main(string[] args)
        {
            NutInfo NutData = new NutInfo();

            /*GA GenAlg = new GA(NutData.NutInfoArr);
            GenAlg.RunGA();*/

            /*DE DiffAlg = new DE(NutData.NutInfoArr);
            DiffAlg.RunDA();*/

            PSO SwarmAlg = new PSO(NutData.NutInfoArr);
            SwarmAlg.RunPSO();
        }
    }
}
