using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WRCI411_Ass4_Collier
{
    class NutInfo
    {
        private double[,] nutInfoArr = new double[77, 10]; //Nutritional info for all foods per $
        public double[,] NutInfoArr { get { return nutInfoArr; } }

        public NutInfo()
        {
            int row = 0;
            StreamReader SR = new StreamReader("DietProblem.csv");//Read from Bin->Debug
            string[] headings = (SR.ReadLine()).Split(',');
            while (!SR.EndOfStream)
            {
                string[] entries = (SR.ReadLine()).Split(',');
                for (int col = 1; col <= nutInfoArr.GetLength(1); col++)
                {
                    nutInfoArr[row, col - 1] = Convert.ToDouble((entries[col]));
                }
                row++;
            }
            SR.Close();
        }
    }
}
