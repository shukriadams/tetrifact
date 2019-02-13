function packageDelete(packageId)
{
    if (!window.fetch)
        return alert('This browser does not support AJAX calls, plese upgrade to a modern browser.');

    if (!confirm('Are you sure you want to delete this package? Deleting cannot be undone.'))
        return;
    
    fetch("/v1/packages/" + packageId, { method: "DELETE" })
        .then(function (response) {
            if (response.status === 200)
                return window.location = '/';

            console.log(response);
            alert(response.status);
        });
    
}