using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GeoQuiz.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Infrastructure;

namespace GeoQuiz.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Questions
        public ActionResult Index()
        {
            return View(db.Questions.ToList());
        }
   
        // GET: Questions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Include(q => q.Files).SingleOrDefault(q => q.QuestionId == id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }
        [Authorize]
        // GET: Questions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QuestionId,CorrectAnswer,Incorrect1,Incorrect2,Incorrect3")] Question question, HttpPostedFileBase upload)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (upload.ContentType.ToLower() == "image/jpg" ||
                   upload.ContentType.ToLower() == "image/jpeg" ||
                   upload.ContentType.ToLower() == "image/pjpeg" ||
                   upload.ContentType.ToLower() == "image/gif" ||
                   upload.ContentType.ToLower() == "image/x-png" ||
                   upload.ContentType.ToLower() == "image/png" &&
                   upload != null && upload.ContentLength > 0)
                    {
                        var selfie = new File
                        {
                            FileName = System.IO.Path.GetFileName(upload.FileName),
                            FileType = FileType.Selfie,
                            ContentType = upload.ContentType
                        };

                        using (var reader = new System.IO.BinaryReader(upload.InputStream))
                        {
                            selfie.Content = reader.ReadBytes(upload.ContentLength);
                        }
                        question.Files = new List<File> { selfie };

                    }
                    db.Questions.Add(question);
                    db.SaveChanges();
                    return RedirectToAction("Confirm", new { id = question.QuestionId });
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to upload question. Please try again.");
            }
            return View(question);
        }

        // GET
        [AllowAnonymous]
        public ActionResult Play()
        {
            List<Question> questions = new List<Question>(db.Questions);
            Methods.ShuffleList.ShuffleQuestions(questions);
            Question question = questions[0];
           
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Play(Question question, string answer, string correctAnswer)
        {
            correctAnswer = question.CorrectAnswer;
            if (answer == correctAnswer)
            {
                return RedirectToAction("Correct");            }
            else if (answer != correctAnswer)
            {
                return RedirectToAction("Incorrect");
            }
            else
            {
                return View(question);
            }
        }

        // GET
        [AllowAnonymous]
        public ActionResult Correct()
        {
            if (User.Identity.IsAuthenticated)
            {
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var manager = new UserManager<ApplicationUser>(store);
                var currentUser = manager.FindById(User.Identity.GetUserId());
                currentUser.Score = currentUser.Score + 100;
                var storeContext = store.Context;
                storeContext.SaveChanges();
            }
            return View();
        }

        // GET
        [AllowAnonymous]
        public ActionResult Incorrect()
        {
            if (User.Identity.IsAuthenticated)
            {
                var store = new UserStore<ApplicationUser>(new ApplicationDbContext());
                var manager = new UserManager<ApplicationUser>(store);
                var currentUser = manager.FindById(User.Identity.GetUserId());
                currentUser.Score = currentUser.Score - 50;
                var storeContext = store.Context;
                storeContext.SaveChanges();
            }
            return View();
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionId,CorrectAnswer,Incorrect1,Incorrect2,Incorrect3")] Question question)
        {
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(question);
        }

        // GET: Questions/Delete/5
        public ActionResult Confirm(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Questions.SingleOrDefault(q => q.QuestionId == id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }


        // POST: Questions/Delete/5
        [HttpPost, ActionName("Confirm")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Question question = db.Questions.Find(id);
            db.Questions.Remove(question);
            db.SaveChanges();
            return RedirectToAction("Create");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
