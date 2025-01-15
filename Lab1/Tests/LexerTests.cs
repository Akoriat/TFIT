using Lab1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Lab1.Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void TestAnalyze_WithValidInput_ReturnsExpectedLexemes()
        {
            // Arrange
            Lexer lexer = new();
            string input = "while a > b";

            // Act
            lexer.Analyze(input);
            var lexemes = lexer.GetLexemes();

            // Assert
            Assert.AreEqual(3, lexemes.Count);
            Assert.AreEqual("while", lexemes[0].Value);
            Assert.AreEqual("a", lexemes[1].Value);
            Assert.AreEqual(">", lexemes[2].Value);
        }
    }
}
