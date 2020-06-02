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
using MedicalSupport.ViewModels.Devices;

namespace MedicalSupport.Controllers
{
    [Authorize]
    public class DevicesController : Controller
    {
        private readonly AppDbContext _context;

        public DevicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Devices
        public async Task<IActionResult> Index()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            List<AppUser> allBound = _context.SickWardenRelations.Where(p => p.Warden.Equals(appUser) && p.IsAccepted.Equals(true)).Select(p => p.Sick).ToList();
            allBound.Add(appUser);

            List<Device> devices = await _context.Devices.Include(p => p.Owner).Where(p => allBound.Contains(p.Owner)).ToListAsync();

            List<DeviceIndexViewModel> deviceIndexViewModels = new List<DeviceIndexViewModel>();
            foreach (Device device in devices)
            {
                deviceIndexViewModels.Add(new DeviceIndexViewModel
                {
                    Id = device.Id,
                    Name = device.Name,
                    Comment = device.Comment,
                    Owner = device.Owner.FullName,
                    IsServiceObligatory = device.IsServiceObligatory.GetValueOrDefault(),
                    LastPlaceLeft = device.LastPlaceLeft,
                    LastServiceDate = device.LastServiceDate,
                    LastUsedDate = device.LastUsedDate
                });
            }

            return View(deviceIndexViewModels);
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
            foreach(AppUser current in allBound)
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

        // GET: Devices/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            Device device = await _context.Devices
                .FirstOrDefaultAsync(m => m.Id == id);
            if (device == null)
            {
                return NotFound();
            }

            await _context.Entry(device).Reference(p => p.Owner).LoadAsync();
            if (!device.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(device.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            DeviceIndexViewModel deviceViewModel = new DeviceIndexViewModel
            {
                Id = device.Id,
                Name = device.Name,
                Comment = device.Comment,
                Owner = device.Owner.FullName,
                IsServiceObligatory = device.IsServiceObligatory.GetValueOrDefault(),
                LastPlaceLeft = device.LastPlaceLeft,
                LastServiceDate = device.LastServiceDate,
                LastUsedDate = device.LastUsedDate
            };

            return View(deviceViewModel);
        }

        // GET: Devices/Create
        public IActionResult Create()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            return View();
        }

        // POST: Devices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeviceCreateViewModel deviceViewModel)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            /*if (true)
            {
                ModelState.AddModelError("OwnerId", "Hack attempt!");
                return View(deviceViewModel);
            }*/
            if (ModelState.IsValid)
            {
                AppUser newOwner = null;
                if (appUser.Id.Equals(deviceViewModel.OwnerId))
                {
                    newOwner = appUser;
                }
                else
                {
                    AppUser probablyNewOwner = _context.Users.FirstOrDefault(p => p.Id.Equals(deviceViewModel.OwnerId));
                    if (probablyNewOwner == null)
                    {
                        ModelState.AddModelError("OwnerId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        return View(deviceViewModel);
                    }

                    if (_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(probablyNewOwner) && p.IsAccepted.Equals(true)))
                    {
                        newOwner = probablyNewOwner;
                    }
                    else
                    {
                        ModelState.AddModelError("OwnerId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        return View(deviceViewModel);
                    }
                }

                Device device = new Device
                {
                    Id = deviceViewModel.Id,
                    Name = deviceViewModel.Name,
                    Comment = deviceViewModel.Comment,
                    Owner = newOwner,
                    IsServiceObligatory = deviceViewModel.IsServiceObligatory,
                    LastPlaceLeft = deviceViewModel.LastPlaceLeft,
                    LastServiceDate = deviceViewModel.LastServiceDate,
                    LastUsedDate = deviceViewModel.LastUsedDate
                };

                _context.Add(device);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            return View(deviceViewModel);
        }

        // GET: Devices/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            Device device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound();
            }

            await _context.Entry(device).Reference(p => p.Owner).LoadAsync();
            if(!device.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(device.Owner) && p.IsAccepted.Equals(true))) {
                return NotFound();
            }

            ViewBag.UserList = MakeAllowedUsersList(appUser, device.Owner.Id);

            DeviceCreateViewModel deviceViewModel = new DeviceCreateViewModel
            {
                Id = device.Id,
                Name = device.Name,
                Comment = device.Comment,
                OwnerId = device.Owner.Id,
                IsServiceObligatory = device.IsServiceObligatory.GetValueOrDefault(),
                LastPlaceLeft = device.LastPlaceLeft,
                LastServiceDate = device.LastServiceDate,
                LastUsedDate = device.LastUsedDate
            };

            return View(deviceViewModel);
        }

        // POST: Devices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, DeviceCreateViewModel deviceViewModel)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id != deviceViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Device device = await _context.Devices.FindAsync(id);
                await _context.Entry(device).Reference(p => p.Owner).LoadAsync();
                if (!device.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(device.Owner) && p.IsAccepted.Equals(true)))
                {
                    return NotFound();
                }

                if (!device.Owner.Id.Equals(deviceViewModel.OwnerId))
                {
                    AppUser newOwner = null;
                    if (appUser.Id.Equals(deviceViewModel.OwnerId))
                    {
                        newOwner = appUser;
                    }
                    else
                    {
                        AppUser probablyNewOwner = _context.Users.FirstOrDefault(p => p.Id.Equals(deviceViewModel.OwnerId));
                        if (probablyNewOwner == null)
                        {
                            ModelState.AddModelError("OwnerId", "Hack attempt!");
                            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                            return View(deviceViewModel);
                        }

                        if (_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(probablyNewOwner) && p.IsAccepted.Equals(true)))
                        {
                            newOwner = probablyNewOwner;
                        }
                        else
                        {
                            ModelState.AddModelError("OwnerId", "Hack attempt!");
                            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                            return View(deviceViewModel);
                        }
                    }
                    device.Owner = newOwner;
                }

                device.Name = deviceViewModel.Name;
                device.Comment = deviceViewModel.Comment;
                device.LastUsedDate = deviceViewModel.LastUsedDate;
                device.LastPlaceLeft = deviceViewModel.LastPlaceLeft;
                device.IsServiceObligatory = deviceViewModel.IsServiceObligatory;
                device.LastServiceDate = deviceViewModel.LastServiceDate;

                try
                {
                    _context.Update(device);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeviceExists(device.Id))
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
            ViewBag.UserList = MakeAllowedUsersList(appUser, deviceViewModel.OwnerId);
            return View(deviceViewModel);
        }

        // GET: Devices/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            Device device = await _context.Devices
                .FirstOrDefaultAsync(m => m.Id == id);
            if (device == null)
            {
                return NotFound();
            }

            await _context.Entry(device).Reference(p => p.Owner).LoadAsync();
            if (!device.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(device.Owner) && p.IsAccepted.Equals(true)))
            {
                return RedirectToAction(nameof(Index));
            }

            DeviceIndexViewModel deviceViewModel = new DeviceIndexViewModel
            {
                Id = device.Id,
                Name = device.Name,
                Comment = device.Comment,
                Owner = device.Owner.FullName,
                IsServiceObligatory = device.IsServiceObligatory.GetValueOrDefault(),
                LastPlaceLeft = device.LastPlaceLeft,
                LastServiceDate = device.LastServiceDate,
                LastUsedDate = device.LastUsedDate
            };

            return View(deviceViewModel);
        }

        // POST: Devices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            Device device = await _context.Devices.FindAsync(id);

            await _context.Entry(device).Reference(p => p.Owner).LoadAsync();
            if (!device.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(device.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeviceExists(long id)
        {
            return _context.Devices.Any(e => e.Id == id);
        }
    }
}
