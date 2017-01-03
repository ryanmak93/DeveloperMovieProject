//Open modal
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
};

//close modal
var CloseDialog = function()
{
    $("#dialog").modal('hide');
}

//click on movie title to open dialog
$(".movielink").click(function () {
    OpenDialog("/Movie/" + this.id + "/GetMovie");
});

//autocomplete
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
        OpenDialog("/Movie/" + ui.item.value + "/GetMovie");
    }
});

//confirm deletion
//$('[data-confirm]').click(function (e) {
//    if (!confirm($(this).attr("data-confirm"))) {
//        e.preventDefault();
//    }
//});

var DeleteUser = function(userId)
{
    var options = { "backdrop": "static", keyboard: true };
    $.ajax({
        url: "/Account/IsCurrentUser?userId=" + userId,
        dataType: 'json',
        type: 'GET',
        success: function (data) {
            if(!data)
            {
                if (confirm($("[name=deleteUser]").attr("data-confirm"))) {
                    //call controller delete
                    $.ajax({
                        url: "/Account/" + userId + "/Delete?userId=",
                        dataType: 'json',
                        type: 'GET',
                        success: function (data) {
                            window.location = data;
                        }
                    });
                }
            }
            else
            {
                alert("Can't delete current user")
            }
        }
    });
};

var DeleteGenre = function (genreId, movieCount) {
    if(movieCount == 0)
    {
        if (confirm($("[name=deleteGenre]").attr("data-confirm"))) {
            //call controller delete
            $.ajax({
                url: "/Genre/" + genreId + "/Delete",
                dataType: 'json',
                type: 'GET',
                success: function (data) {
                    window.location = data;
                }
            });
        }
    }
    else
    {
        alert("Genre must not have any movies to be deleted")
    }
};

var DeleteMovie = function (movieId) {
    if (confirm($("[name=deleteMovie]").attr("data-confirm"))) {
        //call controller delete
        $.ajax({
            url: "/Movie/" + movieId + "/Delete",
            dataType: 'json',
            type: 'GET',
            success: function (data) {
                window.location = data;
            }
        });
    }
};

//$("#editMovie, #createMovie").click(function (e) {
//    var movieId = $("#movieId").val();
//    var movieTitle = $("#movieTitle").val();
//    var movieYear = $("#movieYear").val()
//    $.ajax({
//        url: "/Movie/MovieCheck?id=" + movieId + "&title=" + movieTitle + "&year=" + movieYear,
//        dataType: 'json',
//        type: 'GET',
//        success: function (data) {
//            if (!data) {
//                alert("Taken");
//                $("errormsg").val(movieTitle + " (" + movieYear + ") already exists");
//                e.preventDefault();
//            }
//            else {
//                $("errormsg").val("")
//            }
//        }
//    });
//});
var validMovie = true;
$("#movieTitle, #movieYear").on('input', function (e) {
    var movieId = $("#movieId").val();
    var movieTitle = $("#movieTitle").val();
    var movieYear = $("#movieYear").val()
    $.ajax({
        url: "/Movie/MovieCheck?id=" + movieId + "&title=" + movieTitle + "&year=" + movieYear,
        dataType: 'json',
        type: 'GET',
        success: function (data) {
            if (!data) {
                //alert("Taken");
                validMovie = false;
                $("#errormsg").text(movieTitle + " (" + movieYear + ") already exists");
                e.preventDefault();
            }
            else {
                $("#errormsg").text("")
                validMovie = true;
            }
        }
    });
});

$("#editMovie, #createMovie").click(function (e) {
    if(!validMovie)
    {
        e.preventDefault();
    }
});