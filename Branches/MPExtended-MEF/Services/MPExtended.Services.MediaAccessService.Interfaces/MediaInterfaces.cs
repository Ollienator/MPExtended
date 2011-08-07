﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPExtended.Services.MediaAccessService.Interfaces.Movie;
using MPExtended.Services.MediaAccessService.Interfaces.Music;
using MPExtended.Services.MediaAccessService.Interfaces.TVShow;

namespace MPExtended.Services.MediaAccessService.Interfaces
{
    // each interface represents the structure of MEF plugins
    // A new MEF plugin has to implement one of these interfaces in order to be used by the service.

    public interface IMusicLibrary
    {
        IList<WebMusicTrackBasic> GetAllTracks();
        IList<WebMusicAlbumBasic> GetAllAlbums();
        IList<WebMusicArtistBasic> GetAllArtists();
        WebMusicTrackBasic GetTrackBasicById(string trackId);
        WebMusicAlbumBasic GetAlbumBasicById(string albumId);
        WebMusicArtistBasic GetArtistBasicById(string artistId);
        IList<WebMusicTrackBasic> GetTracksByAlbumId(string albumId);        
        IList<String> GetAllGenres();
     
    }
    public interface IMovieLibrary
    {
        IList<WebMovieBasic> GetAllMovies();
        IList<WebMovieDetailed> GetAllMoviesDetailed();
        WebMovieBasic GetMovieBasicById(string movieId);
        WebMovieDetailed GetMovieDetailedById(string movieId);
        IList<String> GetAllGenres();

    }
    public interface ITVShowLibrary
    {
        IList<WebTVShowBasic> GetAllSeries();
         WebTVShowDetailed GetTVShowDetailed(string seriesId);
        IList<WebTVSeason> GetSeasons(string seriesId);
         IList<WebTVEpisodeBasic> GetEpisodes(string seriesId);
         IList<WebTVEpisodeBasic> GetEpisodesForSeason(string seriesId, string seasonId);
         WebTVEpisodeDetailed GetEpisodeDetailed(string episodeId);
    }
    public interface IPictureLibrary
    {

    }
}
