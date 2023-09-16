using System.ComponentModel.DataAnnotations;
using AppMvc.Models.Blog;

namespace AppMvc.Areas.Blog.Models{
    public class CreatPostModel : Post{
        [Display(Name ="Chuyên Mục")]
        public int[] CategoryIDs {get; set;}
    }
}