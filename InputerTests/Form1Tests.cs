using Microsoft.VisualStudio.TestTools.UnitTesting;
using Inputer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inputer.Tests
{
    [TestClass()]
    public class Form1Tests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            Form1 form1 = new Form1("test");

            List<Tuple< List<Tuple<string, bool>>, string>> words = new List<Tuple<List<Tuple<string, bool>>, string>>()
            {
                //Tuple.Create("ghbdtn rfr ltkf","привет как дела"),
                //Tuple.Create(";bnm [jhjij","жить хорошо"),
                //Tuple.Create("gjckt njuj rfr z gjt[fk ljvjq? nj edbltk xnj-nj yjdjt","после того как я поехал домой, то увидел что-то новое"),
            };
            words.Add(
                Tuple.Create(new List<Tuple<string, bool>>
                {
                    Tuple.Create("G",false),
                    Tuple.Create("H",false),
                    Tuple.Create("B",false),
                    Tuple.Create("D",false),
                    Tuple.Create("T",false),
                    Tuple.Create("N",false),
                    Tuple.Create(" ",false),
                    Tuple.Create("R",false),
                    Tuple.Create("F",false),
                    Tuple.Create("R",false),
                    Tuple.Create(" ",false),
                    Tuple.Create("L",false),
                    Tuple.Create("T",false),
                    Tuple.Create("K",false),
                    Tuple.Create("F",false),
                }                    
                , "привет как дела")
                );
            //после того как я поехал домой? то увидел что-то новое
            foreach (var word in words)
            {
                var from = word.Item1;
                var to = word.Item2;

                var result = form1.Convert(from, false);
                Assert.AreEqual(to, result);
            }            
        }
        [TestMethod()]
        public void ConvertTest1()
        {
            Form1 form1 = new Form1("test");
            List<Tuple<List<Tuple<string, bool>>, string>> words = new List<Tuple<List<Tuple<string, bool>>, string>>();
            words.Add(
                Tuple.Create(new List<Tuple<string, bool>>
                {
                    Tuple.Create("G",true),
                    Tuple.Create("H",false),
                    Tuple.Create("B",false),
                    Tuple.Create("D",false),
                    Tuple.Create("T",false),
                    Tuple.Create("N",false),
                    Tuple.Create(" ",false),
                    Tuple.Create("R",false),
                    Tuple.Create("F",false),
                    Tuple.Create("R",false),
                    Tuple.Create(" ",false),
                    Tuple.Create("L",false),
                    Tuple.Create("T",false),
                    Tuple.Create("K",false),
                    Tuple.Create("F",false),
                    Tuple.Create("7",true),
                }
                , "Привет как дела?")
                );
            //после того как я поехал домой? то увидел что-то новое
            foreach (var word in words)
            {
                var from = word.Item1;
                var to = word.Item2;

                var result = form1.Convert(from, false);
                Assert.AreEqual(to, result);
            }
        }

        [TestMethod()]
        public void ConvertTest_ru_to_en_with_spec_chars()
        {
            Form1 form1 = new Form1("test");
            List<Tuple<List<Tuple<string, bool>>, string>> words = new List<Tuple<List<Tuple<string, bool>>, string>>();
            words.Add(
                Tuple.Create(new List<Tuple<string, bool>>
                {
                    Tuple.Create("w",false),
                    Tuple.Create("i",false),
                    Tuple.Create("t",false),
                    Tuple.Create("h",false),
                    Tuple.Create("9",true),
                    Tuple.Create("n",false),
                    Tuple.Create("o",false),
                    Tuple.Create("l",false),
                    Tuple.Create("o",false),
                    Tuple.Create("c",false),
                    Tuple.Create("k",false),
                    Tuple.Create("0",true),
                }
                , "with(nolock)")
                );
            //после того как я поехал домой? то увидел что-то новое
            foreach (var word in words)
            {
                var from = word.Item1;
                var to = word.Item2;

                var result = form1.Convert(from, true);
                Assert.AreEqual(to, result);
            }
        }
    }
}