using Library.Data;
using Library.Web.Models.Branch;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Library.Web.Controllers
{
    public class BranchController : Controller
    {
        private readonly ILibraryBranchService _branch;

        public BranchController(ILibraryBranchService branch)
        {
            _branch = branch;
        }

        public IActionResult Index()
        {
            var branchModels = _branch.GetAll()
                .Select(branch => new BranchDetailModel
                {
                    Id = branch.Id,
                    BranchName = branch.Name,
                    NumberOfAssets = _branch.GetAssetCount(branch.Id),
                    NumberOfPatrons = _branch.GetPatronCount(branch.Id),
                    IsOpen = _branch.IsBranchOpen(branch.Id)
                }).ToList();

            var model = new BranchIndexModel
            {
                Branches = branchModels
            };

            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var branch = _branch.Get(id); 
            var model = new BranchDetailModel
            {
                BranchName = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                Telephone = branch.Telephone,
                BranchOpenedDate = branch.OpenDate.ToString("yyyy-MM-dd"),
                NumberOfPatrons = _branch.GetPatronCount(id),
                NumberOfAssets = _branch.GetAssetCount(id),
                TotalAssetValue = _branch.GetAssetsValue(id),
                ImageUrl = branch.ImageUrl,
                HoursOpen = _branch.GetBranchHours(id)
            };

            return View(model);
        }
    }
}