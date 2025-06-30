using System.ComponentModel.DataAnnotations;

namespace TodoWebApp.Models
{
    /// <summary>
    /// - [Required], [StringLength], [DataType] drive client/server validation automatically
    /// </summary>
    public class TodoItem
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [Required, StringLength(1000)]
        public string Details { get; set; }

        public bool IsDone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EntryDate { get; set; }

        public override string ToString() => $"{Id} => {Title} => {IsDone} => {DueDate}";
    }

}
