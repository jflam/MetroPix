// This is a view model that defines an abstract view
// over a collection of photos. The intent for this
// is to define a generic data source for our UI to
// data bind against regardless of how or where the 
// data came from. Initially the source of data will
// be the 500px site, but there will be many more
// data providers in the future.

using System;
namespace MetroPix
{
    public class PhotoSummary
    {
        public int Id { get; set; }
        public Uri PhotoUri { get; set; }
        public string Caption { get; set; }
        public string Author { get; set; }
        public int Votes { get; set; }
        public double Rating { get; set; }
    }
}