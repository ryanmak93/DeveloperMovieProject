
var OpenDialog = function(url)
{
    var options = { "backdrop": "static", keyboard: true };
    $.ajax({
        url: url,
        dataType: 'html',
        type: 'GET',
        success: function (data) {
            $("#dialogcontent").html(data);
            $("#dialog").modal(
            {
                "backdrop": "static",
                keyboard: true                
            });
            $("#createdialog").modal('show');
        }
    });
}

var CloseDialog = function()
{
    $("#dialog").modal('hide');
}

$(".movielink").click(function () {
    OpenDialog("/Movie/GetMovie?movieId=" + this.id);
});


$('#movieSearch').autocomplete({
    source: function (request, response) {
        $.getJSON("/Movie/Search?search=" + request.term, function (data) {
            response($.map(data, function (item) {
                return {
                    label: item.Title + " (" + item.Year + ")",
                    value: item.MovieID
                };
            }));
        });
    },
    select: function(event, ui){
        OpenDialog("/Movie/GetMovie?movieId=" + ui.item.value) ;
    }
});

//var GetGenreMovies = function(genreId)
//{
//    var genremovies = ko.observableArray();
//    $.ajax({
//        url: "/Movie/GenreMovies?genreId=" + genreId,
//        dataType: 'json',
//        type: "POST",

//        success: function (data) {
//            genremovies = data;
//            ko.applyBindings(genremovies);

//        },
//        error: function(d, textStatus, error) {
//           alert("getJSON failed, status: " + textStatus + ", error: "+error)
//        },
//    });
//    return genremovies;
//};

$('[data-confirm]').click(function (e) {
    if (!confirm($(this).attr("data-confirm"))) {
        e.preventDefault();
    }
});

$("#createGenre, #editGenre").click(function (e) {
    var genreName = $("#genreName").val();
    if($.trim(genreName) == '')
    {
        alert("Missing Name");
        e.preventDefault();
    }
});

$("#createMovie, #editMovie").click(function (e) {
    var movieName = $("#movieTitle").val();
    var movieYear = $("#movieYear").val();
    if ($.trim(movieName) == '') {
        alert("Missing Title");
        e.preventDefault();
    }
    else if (movieYear == 0)
    {
        alert("Missing Year");
        e.preventDefault();
    }
});

$("#editUser").click(function (e) {
    var username = $("#username").val();
    if ($.trim(username) == '') {
        alert("Missing Username");
        e.preventDefault();
    }
});

$("#createUser").click(function (e) {
    var username = $("#username").val();
    var password = $("#password").val();
    if ($.trim(username) == '') {
        alert("Missing Username");
        e.preventDefault();
    }
    else if (password == '') {
        alert("Missing password");
        e.preventDefault();
    }
});