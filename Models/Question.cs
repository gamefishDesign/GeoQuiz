using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GeoQuiz.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        [Required]
        [Display(Name = "Correct Answer")]
        [StringLength(30, MinimumLength = 5)]
        public string CorrectAnswer { get; set; }

        [Required]
        [Display(Name = "Incorrect Answer #1")]
        [StringLength(30, MinimumLength = 5)]
        public string Incorrect1 { get; set; }

        [Required]
        [Display(Name = "Incorrect Answer #2")]
        [StringLength(30, MinimumLength = 5)]
        public string Incorrect2 { get; set; }

        [Required]
        [Display(Name = "Incorrect Answer #3")]
        [StringLength(30, MinimumLength = 5)]
        public string Incorrect3 { get; set; }

        public virtual ICollection<File> Files { get; set; }
    }
}