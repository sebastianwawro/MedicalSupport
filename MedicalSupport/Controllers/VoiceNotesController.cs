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
using MedicalSupport.ViewModels.VoiceNotes;
using System.IO;

namespace MedicalSupport.Controllers
{
    [Authorize]
    public class VoiceNotesController : Controller
    {
        private readonly AppDbContext _context;

        public VoiceNotesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: VoiceNotes
        public async Task<IActionResult> Index()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            List<AppUser> allBound = _context.SickWardenRelations.Where(p => p.Warden.Equals(appUser) && p.IsAccepted.Equals(true)).Select(p => p.Sick).ToList();
            allBound.Add(appUser);

            List<VoiceNote> voiceNotes = await _context.VoiceNotes.Where(p => allBound.Contains(p.Owner)).ToListAsync();

            List<VoiceNoteIndexViewModel> voiceNoteIndexViewModels = new List<VoiceNoteIndexViewModel>();
            foreach (VoiceNote voiceNote in voiceNotes)
            {
                voiceNoteIndexViewModels.Add(new VoiceNoteIndexViewModel
                {
                    Id = voiceNote.Id,
                    Name = voiceNote.Name,
                    Comment = voiceNote.Comment,
                    Owner = voiceNote.Owner.FullName
                });
            }

            return View(voiceNoteIndexViewModels);
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

        // GET: VoiceNotes/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            VoiceNote voiceNote = await _context.VoiceNotes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voiceNote == null)
            {
                return NotFound();
            }

            await _context.Entry(voiceNote).Reference(p => p.Owner).LoadAsync();
            if (!voiceNote.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(voiceNote.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            VoiceNoteIndexViewModel voiceNoteIndexViewModel = new VoiceNoteIndexViewModel
            {
                Id = voiceNote.Id,
                Name = voiceNote.Name,
                Comment = voiceNote.Comment,
                Owner = voiceNote.Owner.FullName
            };

            return View(voiceNoteIndexViewModel);
        }

        // GET: VoiceNotes/Create
        public IActionResult Create()
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            return View();
        }

        // POST: VoiceNotes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(VoiceNoteCreateViewModel voiceNoteViewModel)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);
            if (ModelState.IsValid)
            {
                AppUser newOwner = null;
                if (appUser.Id.Equals(voiceNoteViewModel.OwnerId))
                {
                    newOwner = appUser;
                }
                else
                {
                    AppUser probablyNewOwner = _context.Users.FirstOrDefault(p => p.Id.Equals(voiceNoteViewModel.OwnerId));
                    if (probablyNewOwner == null)
                    {
                        ModelState.AddModelError("OwnerId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        return View(voiceNoteViewModel);
                    }

                    if (_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(probablyNewOwner) && p.IsAccepted.Equals(true)))
                    {
                        newOwner = probablyNewOwner;
                    }
                    else
                    {
                        ModelState.AddModelError("OwnerId", "Hack attempt!");
                        ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                        return View(voiceNoteViewModel);
                    }
                }

                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(voiceNoteViewModel.File.FileName).ToLowerInvariant();
                if (voiceNoteViewModel != null && voiceNoteViewModel.File != null && voiceNoteViewModel.File.Length != 0)
                {
                    if (!Directory.Exists("wwwroot\\UserRecordings"))
                    {
                        Directory.CreateDirectory("wwwroot\\UserRecordings");
                    }

                    var path = Path.Combine(
                            Directory.GetCurrentDirectory(), "wwwroot\\UserRecordings",
                            newFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await voiceNoteViewModel.File.CopyToAsync(stream);
                    }
                }
                else
                {
                    ModelState.AddModelError("File", "Invalid file!");
                    ViewBag.UserList = MakeAllowedUsersList(appUser, null);
                    return View(voiceNoteViewModel);
                }

                VoiceNote voiceNote = new VoiceNote
                {
                    Id = voiceNoteViewModel.Id,
                    Name = voiceNoteViewModel.Name,
                    Comment = voiceNoteViewModel.Comment,
                    Owner = newOwner,
                    FileName = newFileName
                };

                _context.Add(voiceNote);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.UserList = MakeAllowedUsersList(appUser, null);
            return View(voiceNoteViewModel);
        }

        // GET: VoiceNotes/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            VoiceNote voiceNote = await _context.VoiceNotes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voiceNote == null)
            {
                return NotFound();
            }

            await _context.Entry(voiceNote).Reference(p => p.Owner).LoadAsync();
            if (!voiceNote.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(voiceNote.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            VoiceNoteIndexViewModel voiceNoteIndexViewModel = new VoiceNoteIndexViewModel
            {
                Id = voiceNote.Id,
                Name = voiceNote.Name,
                Comment = voiceNote.Comment,
                Owner = voiceNote.Owner.FullName
            };

            return View(voiceNoteIndexViewModel);
        }

        // POST: VoiceNotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            VoiceNote voiceNote = await _context.VoiceNotes.FindAsync(id);

            await _context.Entry(voiceNote).Reference(p => p.Owner).LoadAsync();
            if (!voiceNote.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(voiceNote.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            VoiceNoteIndexViewModel voiceNoteIndexViewModel = new VoiceNoteIndexViewModel
            {
                Id = voiceNote.Id,
                Name = voiceNote.Name,
                Comment = voiceNote.Comment,
                Owner = voiceNote.Owner.FullName
            };

            _context.VoiceNotes.Remove(voiceNote);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoiceNoteExists(long id)
        {
            return _context.VoiceNotes.Any(e => e.Id == id);
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".mp4", "video/mp4"},
                {".mp3", "audio/mpeg"},
                {".wma", "audio/x-ms-wma"},
                {".wav", "audio/x-wav"}
            };
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        public async Task<IActionResult> Download(long? id)
        {
            AppUser appUser = AuthExtender.GetLoggedInUser(this, _context);

            if (id == null)
            {
                return NotFound();
            }

            VoiceNote voiceNote = await _context.VoiceNotes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voiceNote == null)
            {
                return NotFound();
            }

            await _context.Entry(voiceNote).Reference(p => p.Owner).LoadAsync();
            if (!voiceNote.Owner.Equals(appUser) & !_context.SickWardenRelations.Any(p => p.Warden.Equals(appUser) && p.Sick.Equals(voiceNote.Owner) && p.IsAccepted.Equals(true)))
            {
                return NotFound();
            }

            var path = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot\\UserRecordings", voiceNote.FileName);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), voiceNote.Name + Path.GetExtension(path).ToLowerInvariant());
        }
    }
}
