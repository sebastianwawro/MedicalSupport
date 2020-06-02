using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalSupport.Data;
using MedicalSupport.Entities;
using MedicalSupport.Helpers;
using MedicalSupport.ViewModels.SickWardenRelations;

namespace MedicalSupport.Controllers
{
    public class SickWardenRelationsController : Controller
    {
        private readonly AppDbContext _context;

        public SickWardenRelationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: SickWardenRelations
        public async Task<IActionResult> Index()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            List<SickWarden> wardenRawList = await _context.SickWardenRelations.Include(p => p.Warden).Where(p => p.Sick.Equals(appUser)).ToListAsync();
            List<SickWarden> sickRawList = await _context.SickWardenRelations.Include(p => p.Sick).Where(p => p.Warden.Equals(appUser)).ToListAsync();

            List<SickWardenIndexWardenElementViewModel> wardenList = new List<SickWardenIndexWardenElementViewModel>();
            List<SickWardenIndexSickElementViewModel> sickList = new List<SickWardenIndexSickElementViewModel>();

            foreach (SickWarden warden in wardenRawList)
            {
                wardenList.Add(new SickWardenIndexWardenElementViewModel
                {
                    Id = warden.Id,
                    FullName = warden.Warden.FullName,
                    IsAccepted = warden.IsAccepted.GetValueOrDefault(),
                    CanAccept = warden.IsProposedByWarden.GetValueOrDefault(),
                });
            }
            foreach (SickWarden sick in sickRawList)
            {
                sickList.Add(new SickWardenIndexSickElementViewModel
                {
                    Id = sick.Id,
                    FullName = sick.Sick.FullName,
                    IsAccepted = sick.IsAccepted.GetValueOrDefault(),
                    CanAccept = !sick.IsProposedByWarden.GetValueOrDefault()
                });
            }

            SickWardenIndexViewModel sickWardenIndexViewModel = new SickWardenIndexViewModel
            {
                WardenList = wardenList,
                SickList = sickList
            };

            return View(sickWardenIndexViewModel);
        }

        private List<SelectListItem> MakeAllowedUsersList(AppUser user, String SelectedId)
        {
            List<SelectListItem> allChoices = new List<SelectListItem>();
            allChoices.Add(new SelectListItem
            {
                Value = "-1",
                Text = "Select user to relate with",
                Selected = SelectedId == null ? true : false,
                Disabled = true
            });


            List<AppUser> allUsers = _context.Users.ToList();
            List<AppUser> avoidUsers = new List<AppUser>();
            List<AppUser> avoidUsersWarden = _context.SickWardenRelations.Where(p => p.Warden.Equals(user)).Select(p => p.Sick).ToList();
            List<AppUser> avoidUsersSick = _context.SickWardenRelations.Where(p => p.Sick.Equals(user)).Select(p => p.Warden).ToList();
            avoidUsers.AddRange(avoidUsersWarden);
            avoidUsers.AddRange(avoidUsersSick);

            foreach (AppUser current in allUsers)
            {
                if (user.Equals(current)) continue;
                Boolean isAlreadyRelated = avoidUsers.Contains(current);

                allChoices.Add(new SelectListItem
                {
                    Value = current.Id,
                    Text = isAlreadyRelated ? current.FullName + " (already related)" : current.FullName,
                    Selected = SelectedId == null ? false : SelectedId.Equals(current.Id) ? true : false,
                    Disabled = isAlreadyRelated
                });
            }

            return allChoices;
        }

        private List<SelectListItem> MakeTypeList()
        {
            List<SelectListItem> allChoices = new List<SelectListItem>();
            allChoices.Add(new SelectListItem
            {
                Value = "-1",
                Text = "Select who you are",
                Selected = true,
                Disabled = true
            });
            allChoices.Add(new SelectListItem
            {
                Value = "sick",
                Text = "I am sick",
                Selected = false,
                Disabled = false
            });
            allChoices.Add(new SelectListItem
            {
                Value = "warden",
                Text = "I am warden",
                Selected = false,
                Disabled = false
            });
            return allChoices;
        }

        // GET: SickWardenRelations/Create
        public IActionResult Create()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            ViewBag.TypeList = MakeTypeList();
            return View();
        }

        // POST: SickWardenRelations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SickWardenAddViewModel sickWardenAddViewModel)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            if (ModelState.IsValid)
            {
                Boolean isSick = sickWardenAddViewModel.TypeId.Equals("sick");
                AppUser anotherUser = _context.Users.FirstOrDefault(p => p.Id.Equals(sickWardenAddViewModel.UserId));
                if (anotherUser == null)
                {
                    ModelState.AddModelError("UserId", "Hack attempt!");
                    ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                    ViewBag.TypeList = MakeTypeList();
                    return View(sickWardenAddViewModel);
                }
                if (isSick)
                {
                    if (_context.SickWardenRelations.Any(p => p.Sick.Equals(appUser) && p.Warden.Equals(anotherUser)))
                    {
                        ModelState.AddModelError("UserId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        ViewBag.TypeList = MakeTypeList();
                        return View(sickWardenAddViewModel);
                    }
                }
                else
                {
                    if (_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(anotherUser)))
                    {
                        ModelState.AddModelError("UserId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        ViewBag.TypeList = MakeTypeList();
                        return View(sickWardenAddViewModel);
                    }
                }

                SickWarden sickWarden = new SickWarden
                {
                    Sick = isSick ? appUser : anotherUser,
                    Warden = isSick ? anotherUser : appUser,
                    IsAccepted = false,
                    IsProposedByWarden = !isSick
                };

                _context.Add(sickWarden);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            ViewBag.TypeList = MakeTypeList();
            return View(sickWardenAddViewModel);
        }

        // GET: SickWardenRelations/Accept/5
        public async Task<IActionResult> Accept(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            SickWarden sickWarden = await _context.SickWardenRelations.FindAsync(id);
            if (sickWarden == null)
            {
                return NotFound();
            }

            if (sickWarden.Sick.Equals(appUser) && sickWarden.IsProposedByWarden.GetValueOrDefault())
            {
                sickWarden.IsAccepted = true;
                _context.SickWardenRelations.Update(sickWarden);
                await _context.SaveChangesAsync();
            }
            else if (sickWarden.Warden.Equals(appUser) && !sickWarden.IsProposedByWarden.GetValueOrDefault())
            {
                sickWarden.IsAccepted = true;
                _context.SickWardenRelations.Update(sickWarden);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: SickWardenRelations/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            SickWarden sickWarden = await _context.SickWardenRelations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sickWarden == null)
            {
                return NotFound();
            }

            await _context.Entry(sickWarden).Reference(p => p.Sick).LoadAsync();
            await _context.Entry(sickWarden).Reference(p => p.Warden).LoadAsync();

            SickWardenDeleteViewModel viewModel = new SickWardenDeleteViewModel
            {
                Id = sickWarden.Id,
                FullName = sickWarden.Warden.Equals(appUser) ? sickWarden.Sick.FullName : sickWarden.Warden.FullName
            };

            return View(viewModel);
        }

        // POST: SickWardenRelations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            SickWarden sickWarden = await _context.SickWardenRelations.FindAsync(id);
            await _context.Entry(sickWarden).Reference(p => p.Sick).LoadAsync();
            await _context.Entry(sickWarden).Reference(p => p.Warden).LoadAsync();

            if (sickWarden.Warden.Equals(appUser) || sickWarden.Sick.Equals(appUser))
            {
                _context.SickWardenRelations.Remove(sickWarden);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SickWardenExists(long id)
        {
            return _context.SickWardenRelations.Any(e => e.Id == id);
        }
    }
}
