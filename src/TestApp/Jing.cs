using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace TestApp
{
    public class Jing : IRequest
    {
        public string Message { get; set; }
    }
}
