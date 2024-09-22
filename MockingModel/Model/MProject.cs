using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockAllInOne.MockingModel.Model
{
    public class MProject
    {
        private List<MContainer> _containers;

        public MProject()
        {
            _containers = new List<MContainer>();
        }

        public void AddContainer(MContainer container) { _containers.Add(container); }
    }
}
