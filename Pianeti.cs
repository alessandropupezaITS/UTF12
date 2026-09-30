using System;
using System.Collections.Generic;
using System.Text;

namespace ProgettoUSF12.DBModels
{
    internal class Pianeti
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string diametro { get; set; }

        public string periodo_orbitale { get; set; }

        public string gravità { get; set; }

        public string popolazione { get; set; }

        public string clima { get; set; }

        public string superficie_acqua { get; set; }

        public string terreno { get; set; }

    }
}
