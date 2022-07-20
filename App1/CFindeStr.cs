using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public class CFindeStr
    {
        public bool CheckRecv(char[] Recv, int Length, char[] Target ,int TargetLen)
        {
            bool cz = false;
            if ((Length - TargetLen) < 0) return false;

            for (int i = 0; i < Length - TargetLen + 1; i++) // 一样长时要检查一次
            {
                int j;
                for (j = 0; j < TargetLen; j++)
                {
                    if (Recv[i + j] != Target[j])
                    {
                        break;
                    }
                }

                if (j == TargetLen) //长度足够
                {
                    cz = true;
                    break;
                }
            }
            return cz;
        }

        private void Test_Sub(string str,string tag)
        {
            bool cz = CheckRecv(str.ToCharArray(), str.Length, tag.ToCharArray(),tag.Length);
            bool cz2 = str.Contains(tag);
            if (cz == cz2)
                Console.WriteLine($"判断正确: {str}存在{tag}:{cz} ");
            else
                Console.WriteLine($"判断错误: {str}存在{tag}:{cz} ,实际为:{cz2}");
        }
        public void Test()
        {
            Test_Sub("abcdasd","abc");
            Test_Sub("a", "abc");
            Test_Sub("abc", "abc");
            Test_Sub("ABC","KAKSD");
            Test_Sub("abcd", "abc");
            Test_Sub("aabcdddZDBUSSdabcvdasabc", "abc");
            Test_Sub("asyudasdZDBUSShjsajhdbhasdhiasedhj", "abc");
            Test_Sub("aaabbbabcsadd", "abc");
            Test_Sub("aaabbba bcsadd", "abc");
            Test_Sub("aaabZDBUSSbba b", "abc");
            Test_Sub("aaabbbab", "abc");
            Test_Sub("aaabZDBUSSbbab", "abc");
            Test_Sub("cbaaabbbab", "abc");
        }


    }
}
