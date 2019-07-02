$(function () {

    //Set the hubs URL for the connection
    var hub = $.connection.hub;
    var myHub = $.connection.signalRHub;
    hub.url = "/signalr";

    var playerStatus = {};
    var radiosList = [];

    var youtubeResult = [];

    function formatSongTitle(song)
    {
        if (song != null)
        {
            var title = song.Title;
            if (song.Author != null) {
                title += ' (by ' + song.Author.Name + ')';
            }
            title += '<span>' + song.SongLength + '</span>';
            return title;
        }
        return '';
    }

    // Create a function that the hub can call to broadcast messages.
    myHub.client.refresh = function (status) {
        playerStatus = status;
        SetSongTimer(status.CurrentSong);

        $('.current-title').html(formatSongTitle(status.CurrentSong));

        var queueHtml = '';
        $.each(status.Queue, function (i, item) {
            queueHtml += '<div>' + (i + 1) + '. ' + formatSongTitle(item) + '</div>';
        });
        $('.songs-playlist').html(queueHtml);

        var currentRadioKey = playerStatus.CurrentRadio != null ? playerStatus.CurrentRadio.Key : '';
        $('.radio-playlist > div').removeClass('current');
        $('.radio-playlist > div[data-key="' + currentRadioKey + '"]').addClass('current');

        var currentMode = status.Mode == 0 ? 'r' : 'y';
        $('input[name="pmode"]').filter('[value="' + currentMode + '"]').prop('checked', true);
        if (currentMode == 'y') {
            jQuery('.songs-playlist').show();
            jQuery('.radio-playlist').hide();
            jQuery('#radiosLabel').show();
        };
        if (currentMode == 'r') {
            jQuery('.songs-playlist').hide();
            jQuery('.radio-playlist').show();
            jQuery('#radiosLabel').hide();
        };

        if (status.YouTubeEndRadio != null) {
            $('#playRadioAfterYoutube').prop('checked', true);
            $('#radiosAfterYoutubeList').val(status.YouTubeEndRadio.Key);
        } else {
            $('#playRadioAfterYoutube').prop('checked', false);
        }
    };

    myHub.client.setradios = function (radios) {
        radiosList = radios;
        var radioHtml = '';
        var radioSelectHtml = '';
        var currentRadioKey = playerStatus.CurrentRadio != null ? playerStatus.CurrentRadio.Key : '';
        $.each(radios, function (i, item) {
            var isCurrent = item.Key == currentRadioKey;
            radioHtml += '<div ' + (isCurrent ? 'class="current" ' : '') + ' data-key="' + item.Key + '" onclick="SelectRadio(this)" ondblclick="StartRadio(\'' + item.Key + '\')">' + (i + 1) + '. ' + item.Title + '</div>';
            radioSelectHtml += '<option value="' + item.Key + '" ' + (isCurrent ? 'selected' : '') + '>' + item.Title + '</option>';
        });
        $('.radio-playlist').html(radioHtml);
        $('#radiosAfterYoutubeList').html(radioSelectHtml);
    };

    myHub.client.consolelog = function (log) {
        $('<div></div>').text(log).appendTo('.console .logs');
        $('.console').scrollTop($('.console').prop("scrollHeight"));
    };

    var doForce = false;

    function Init() {
        // Start the connection.
        hub.start().done(function () {
            Bind('play', Play);
            Bind('stop', Stop);
            Bind('pause', Pause);
            Bind('next', Next);
            Bind('QueueSond', function () {
                QueueSong($(this).attr('w'));
            });
            $("#searchText").on('keyup', function (e) {
                if (e.keyCode == 13) { // enter
                    var selectedSong = $('#response > div.selected');
                    if (selectedSong.length == 0) {
                        selectedSong = $('#response > div:first-child');
                    };
                    if (selectedSong.length > 0) {
                        QueueSong(selectedSong[0].getAttribute('data-video'));
                    }
                } else if (e.keyCode == 38) { // up
                    var index = +$('#response > div.selected').data('index') || 0;
                    if (index > 0)
                        index--;
                    $('#response > div.selected').removeClass('selected');
                    $('#response > div[data-index="' + index + '"]').addClass('selected');
                } else if (e.keyCode == 40) { // down
                    var index = 0;
                    if ($('#response > div.selected').length > 0) {
                        index = +$('#response > div.selected').data('index');
                        if (index < ($('#response > div').length - 1))
                            index++;
                    };
                    $('#response > div.selected').removeClass('selected');
                    $('#response > div[data-index="' + index + '"]').addClass('selected');
                } else {
                    var val = $('#searchText').val();
                    if (val.length > 1) {
                        Search($('#searchText').val());
                    };
                };
            });
        });
    };

    function Bind(className, method) {
        $('.' + className).click(method);
    };

    function Call(command) {
        myHub.server.command(command);
    };

    function Play() {
        Call('play');
    };

    function Pause() {
        Call('pause');
    };

    function Stop() {
        Call('stop');
    };

    function Next() {
        Call('next');
    };

    function QueueSong(w) {
        Call(doForce ? 'force:' + w : 'w:' + w);
        $("#searchText").val('');
        $("#response").html('').hide();
    };

    window.Queue = function (w) {
        QueueSong(w);
    };

    window.StartRadio = function (radioName) {
        Call('r:' + radioName);
    };

    window.SelectRadio = function (el) {
        $('.playlist > div').removeClass('selected');
        $(el).addClass('selected');
    };

    window.ChangeMode = function (mode) {
        if (mode == 'y') {
            jQuery('.songs-playlist').show();
            jQuery('.radio-playlist').hide();
            jQuery('#radiosLabel').show();
        };
        if (mode == 'r') {
            jQuery('.songs-playlist').hide();
            jQuery('.radio-playlist').show();
            jQuery('#radiosLabel').hide();
        };
        Call('m:' + mode);
    };

    window.ChangeYouTubeEndMode = function () {
        var selected = $('#playRadioAfterYoutube').is(":checked");
        var selectedRadio = $('#radiosAfterYoutubeList').val();

        if (selected && selectedRadio && selectedRadio.length > 0) {
            Call('yer:' + selectedRadio);
        } else {
            Call('yer:');
        }
    };

    window.SendCommand = function (e, element) {
        if (e.keyCode == 13) {
            e.preventDefault();
            Call(element.value);
            element.value = "";
        };
    };

    // Your use of the YouTube API must comply with the Terms of Service:
    // https://developers.google.com/youtube/terms
    // Called automatically when JavaScript client library is loaded.
    /*window.onClientLoad = function () {
        gapi.client.load('youtube', 'v3', function () {
            // This API key is intended for use only in this lesson.
            // See https://goo.gl/PdPA1 to get a key for your own applications.
            gapi.client.setApiKey('AIzaSyCR5In4DZaTP6IEZQ0r1JceuvluJRzQNLE');
        });
    };*/
    var searchTimer = -1;
    function Search(searchText) {
        if (searchTimer > 0) {
            clearTimeout(searchTimer);
        };
        searchTimer = setTimeout(function () {
            searchTimer = -1;
            SearchCore(searchText);
        }, 500);
    };

    function SearchCore(searchText) {
        // Use the JavaScript client library to create a search.list() API call.
        var request = gapi.client.youtube.search.list({
            q: searchText,
            part: 'snippet',
            type: 'video',
            maxResults: 5
        });

        // Send the request to the API server,
        // and invoke onSearchRepsonse() with the response.
        request.execute(function (response) {
            var result = '';
            youtubeResult.length = 0;
            $.each(response.items, function (i, item) {
                youtubeResult.push(item);
                result = result + RenderResult(i, item);
            });
            $('#response').html(result).show();
        });
    };

    function RenderResult(i, item) {
        return '' +
        '<div data-index="' + i + '" data-video="' + item.id.videoId + '" onclick="Queue(\'' + item.id.videoId + '\');">' +
            '<img src="' + item.snippet.thumbnails.default.url + '" />' +
            '<span>' + item.snippet.title + '</span>' +
        '</div>';
    };

    _songTimerId = -1;
    function SetSongTimer(currentSong) {

        if (_songTimerId > -1) {
            clearInterval(_songTimerId);
            _songTimerId = -1;
            $('.player .timer').html('00:00');
        };

        if (currentSong != null) {
            var leftSec = currentSong.Length - currentSong.CurrentSecond;
            if (leftSec < 0) {
                leftSec = 0;
            }
            var leftMin = Math.floor(leftSec / 60);
            if (leftMin < 0) {
                leftMin = 0;
            }
            leftSec = leftSec % 60;
            var sLeftMin = leftMin > 9 ? leftMin + "" : "0" + leftMin;
            var sLeftSec = leftSec > 9 ? leftSec + "" : "0" + leftSec;
            $('.player .timer').html(sLeftMin + ":" + sLeftSec);

            // count seconds only if state = Playing and Mode = Youtube
            if (playerStatus.State == 1 && playerStatus.Mode == 1) {
                _songTimerId = setInterval(function () {
                    // get current time and minus one second
                    var cTime = $('.player .timer').html();
                    var cMin = +(cTime.split(':')[0]);
                    var cSec = +(cTime.split(':')[1]);
                    if (cMin == 0 && cSec == 0) {
                        clearInterval(_songTimerId);
                        return;
                    };
                    if (cSec == 0) {
                        cMin--;
                        cSec = 59;
                    } else {
                        cSec--;
                    };
                    var csMin = cMin > 9 ? cMin + "" : "0" + cMin;
                    var csSec = cSec > 9 ? cSec + "" : "0" + cSec;
                    $('.player .timer').html(csMin + ":" + csSec);
                }, 1000);
            }
        };
    };

    Init();
});