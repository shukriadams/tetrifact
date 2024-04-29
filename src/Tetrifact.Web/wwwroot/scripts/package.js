function removePackage() {
    let packageId = document.querySelector('.packageId').value

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

async function addTag() {
    let packageId = document.querySelector('.packageId').value
    let tag = document.querySelector('.newTag').value;

    if (!tag) {
        alert('Tag is required.');
        return;
    }

    fetch(`/v1/tags/${encodeURIComponent(tag)}/${packageId}`, { method: "POST" })
        .then(function () {
            window.location.reload(true);
        });
}


async function removeTag(e) {
    let tag = e.target.getAttributeNode('data-tag').value;
    let packageId = document.querySelector('.packageId').value

    if (!confirm(`Are you sure you want to remove the tag "${tag}"?`))
        return;

    fetch(`/v1/tags/${encodeURIComponent(tag)}/${packageId}`, { method: "DELETE" })
        .then(function () {
            window.location.reload(true);
        });
}

async function onClick(e) {
    if (e.target.classList && e.target.classList.contains('addTag'))
        await addTag(e);
    else if (e.target.classList && e.target.classList.contains('package-removeTag'))
        await removeTag(e);
    else if (e.target.classList && e.target.classList.contains('package-removePackage'))
        await removePackage();
}

document.addEventListener('click', onClick, false);

(() => {
    const packageId = document.querySelector('.packageId').value,
        statusNode = document.querySelector('[data-archiveStatus]'),
        url = `/archiveStatus/${packageId}`

    function checkStatus() {
        
    }

    let timer = window.setInterval(checkStatus, 1000)

})()
