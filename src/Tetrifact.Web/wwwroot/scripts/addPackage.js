(async function () {

    let filesSelector = document.querySelector('[name=Files]'),
        form = document.querySelector('form[id=archiveUpload]'),
        submit = form.querySelector('.submit'),
        contactingServer = document.querySelector('.addPackage-contactingServer'),
        packageName = document.querySelector('.packageName'),
        projectName = document.querySelector('.projectName');

    filesSelector.addEventListener('change', function () {
        // If a package name hasn't been entered, auto-fills the package name using the file selected, based on file name.
        if (filesSelector.files.length && !packageName.value.length) {
            let name = filesSelector.files[0].name;
            let matches = filesSelector.files[0].name.match(/(.*)[.](.*)/);
            if (matches.length === 3) {
                name = matches[1];
            }

            packageName.value = name;
        }
    }, false);

    submit.addEventListener('click', async function () {
        const formData = new FormData(form),
            package = encodeURIComponent(packageName.value),
            project = encodeURIComponent(projectName.value),
            files = filesSelector.files;

        if (!files.length)
            return alert('Please select an archive to upload.');

        if (!package)
            return alert('Package name is required.');

        submit.classList.add('button--disabled');
        contactingServer.classList.add('addPackage-contactingServer--show');

        try {

            const response = await fetch(`/v1/packages/${project}/${package}?isArchive=true`, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                window.location = `/package/${project}/${package}`;
            } else {
                console.log(response);
            }
        } catch (err) {
            alert('Error:', err);
        } finally {
            contactingServer.classList.remove('addPackage-contactingServer--show');
            submit.classList.remove('button--disabled');
        }
    }, false);

})()