using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalSupport.Data;
using MedicalSupport.Entities;
using Microsoft.AspNetCore.Authorization;
using MedicalSupport.Helpers;
using MedicalSupport.ViewModels.Materials;

namespace MedicalSupport.Controllers
{
    [Authorize]
    public class MaterialsController : Controller
    {
        private readonly AppDbContext _context;

        public MaterialsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Materials
        public async Task<IActionResult> Index()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            List<AppUser> allBound = _context.SickWardenRelations.Where(p => p.Warden.Equals(appUser) && p.IsAccepted.Equals(true)).Select(p => p.Sick).ToList();
            allBound.Add(appUser);

            List<Material> materials = await _context.Materials.Where(p => allBound.Contains(p.Owner)).ToListAsync();

            List<MaterialIndexViewModel> materialIndexViewModels = new List<MaterialIndexViewModel>();
            foreach (Material material in materials)
            {
                materialIndexViewModels.Add(new MaterialIndexViewModel
                {
                    Id = material.Id,
                    Name = material.Name,
                    Comment = material.Comment,
                    Owner = material.Owner.FullName,
                    LastPlaceLeft = material.LastPlaceLeft,
                    LastUsedDate = material.LastUsedDate,
                    IsMedicine = material.IsMedicine.GetValueOrDefault(),
                    Amount = material.Amount,
                    MedicineDose = material.MedicineDose,
                    Type = material.Type
                });
            }

            return View(materialIndexViewModels);
        }

        private List<SelectListItem> MakeAllowedUsersList(AppUser user, String SelectedId)
        {
            List<SelectListItem> allChoices = new List<SelectListItem>();
            allChoices.Add(new SelectListItem
            {
                Value = "-1",
                Text = "Select owner",
                Selected = SelectedId == null ? true : false,
                Disabled = true
            });
            allChoices.Add(new SelectListItem
            {
                Value = user.Id,
                Text = user.FullName,
                Selected = SelectedId == null ? true : SelectedId.Equals(user.Id) ? true : false
            });


            List<AppUser> allBound = _context.SickWardenRelations.Where(p => p.Warden.Equals(user) && p.IsAccepted.Equals(true)).Select(p => p.Sick).ToList();
            foreach (AppUser current in allBound)
            {
                allChoices.Add(new SelectListItem
                {
                    Value = current.Id,
                    Text = current.FullName,
                    Selected = SelectedId == null ? false : SelectedId.Equals(current.Id) ? true : false
                });
            }

            return allChoices;
        }

        // GET: Materials/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            Material material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            await _context.Entry(material).Reference(p => p.Owner).LoadAsync();
            if (!material.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(material.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            MaterialIndexViewModel materialViewModel = new MaterialIndexViewModel
            {
                Id = material.Id,
                Name = material.Name,
                Comment = material.Comment,
                Owner = material.Owner.FullName,
                LastPlaceLeft = material.LastPlaceLeft,
                LastUsedDate = material.LastUsedDate,
                IsMedicine = material.IsMedicine.GetValueOrDefault(),
                Amount = material.Amount,
                MedicineDose = material.MedicineDose,
                Type = material.Type
            };

            return View(materialViewModel);
        }

        // GET: Materials/Create
        public IActionResult Create()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            return View();
        }

        // POST: Materials/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialCreateViewModel materialViewModel)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            if (ModelState.IsValid)
            {
                AppUser newOwner = null;
                if (appUser.Id.Equals(materialViewModel.OwnerId))
                {
                    newOwner = appUser;
                }
                else
                {
                    AppUser probablyNewOwner = _context.Users.FirstOrDefault(p => p.Id.Equals(materialViewModel.OwnerId));
                    if (probablyNewOwner == null)
                    {
                        ModelState.AddModelError("OwnerId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        return View(materialViewModel);
                    }

                    if (_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(probablyNewOwner) && p.IsAccepted.Equals(true)))
                    {
                        newOwner = probablyNewOwner;
                    }
                    else
                    {
                        ModelState.AddModelError("OwnerId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        return View(materialViewModel);
                    }
                }

                Material material = new Material
                {
                    Id = materialViewModel.Id,
                    Name = materialViewModel.Name,
                    Comment = materialViewModel.Comment,
                    Owner = newOwner,
                    LastPlaceLeft = materialViewModel.LastPlaceLeft,
                    LastUsedDate = materialViewModel.LastUsedDate,
                    IsMedicine = materialViewModel.IsMedicine,
                    Amount = materialViewModel.Amount,
                    MedicineDose = materialViewModel.MedicineDose,
                    Type = materialViewModel.Type
                };

                _context.Add(material);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            return View(materialViewModel);
        }

        // GET: Materials/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            await _context.Entry(material).Reference(p => p.Owner).LoadAsync();
            if (!material.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(material.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            ViewBag.UserList = MakeAllowedUsersList(appUser, material.Owner.Id);

            MaterialCreateViewModel materialViewModel = new MaterialCreateViewModel
            {
                Id = material.Id,
                Name = material.Name,
                Comment = material.Comment,
                OwnerId = material.Owner.Id,
                LastPlaceLeft = material.LastPlaceLeft,
                LastUsedDate = material.LastUsedDate,
                IsMedicine = material.IsMedicine.GetValueOrDefault(),
                Amount = material.Amount,
                MedicineDose = material.MedicineDose,
                Type = material.Type
            };

            return View(materialViewModel);
        }

        // POST: Materials/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, MaterialCreateViewModel materialViewModel)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id != materialViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Material material = await _context.Materials.FindAsync(id);
                await _context.Entry(material).Reference(p => p.Owner).LoadAsync();
                if (!material.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(material.Owner) && p.IsAccepted.Equals(true)))
                {
                    return NotFound();
                }

                if (!material.Owner.Id.Equals(materialViewModel.OwnerId))
                {
                    AppUser newOwner = null;
                    if (appUser.Id.Equals(materialViewModel.OwnerId))
                    {
                        newOwner = appUser;
                    }
                    else
                    {
                        AppUser probablyNewOwner = _context.Users.FirstOrDefault(p => p.Id.Equals(materialViewModel.OwnerId));
                        if (probablyNewOwner == null)
                        {
                            ModelState.AddModelError("OwnerId", "Hack attempt!");
                            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                            return View(materialViewModel);
                        }

                        if (_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(probablyNewOwner) && p.IsAccepted.Equals(true)))
                        {
                            newOwner = probablyNewOwner;
                        }
                        else
                        {
                            ModelState.AddModelError("OwnerId", "Hack attempt!");
                            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                            return View(materialViewModel);
                        }
                    }
                    material.Owner = newOwner;
                }

                material.Name = materialViewModel.Name;
                material.Comment = materialViewModel.Comment;
                material.LastUsedDate = materialViewModel.LastUsedDate;
                material.LastPlaceLeft = materialViewModel.LastPlaceLeft;
                material.IsMedicine = materialViewModel.IsMedicine;
                material.MedicineDose = materialViewModel.MedicineDose;
                material.Amount = materialViewModel.Amount;
                material.Type = materialViewModel.Type;

                try
                {
                    _context.Update(material);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialExists(material.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserList = MakeAllowedUsersList(appUser, materialViewModel.OwnerId);
            return View(materialViewModel);
        }

        // GET: Materials/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            Material material = await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id);
            if (material == null)
            {
                return NotFound();
            }

            await _context.Entry(material).Reference(p => p.Owner).LoadAsync();
            if (!material.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(material.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            MaterialIndexViewModel materialViewModel = new MaterialIndexViewModel
            {
                Id = material.Id,
                Name = material.Name,
                Comment = material.Comment,
                Owner = material.Owner.FullName,
                LastPlaceLeft = material.LastPlaceLeft,
                LastUsedDate = material.LastUsedDate,
                IsMedicine = material.IsMedicine.GetValueOrDefault(),
                Amount = material.Amount,
                MedicineDose = material.MedicineDose,
                Type = material.Type
            };

            return View(materialViewModel);
        }

        // POST: Materials/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            Material material = await _context.Materials.FindAsync(id);

            await _context.Entry(material).Reference(p => p.Owner).LoadAsync();
            if (!material.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(material.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialExists(long id)
        {
            return _context.Materials.Any(e => e.Id == id);
        }
    }
}
