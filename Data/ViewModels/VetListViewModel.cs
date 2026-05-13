using Pets_friends.Models;
using System.Collections.Generic;

namespace Pets_friends.Data.ViewModels 
{
    public class VetListViewModel
    {
        public List<VetProfile> Vets { get; set; } = new List<VetProfile>();
        public string? SearchCity { get; set; }
    }
}