using System.Collections.Generic;
using Doppler.CloverAPI.Entities.Clover;

namespace Doppler.CloverAPI.Response
{
    public class GetChargesResponse
    {
        public IList<Charge> Data { get; set; }
    }
}
