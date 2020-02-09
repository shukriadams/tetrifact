(async function () {

    let archiveSelector = document.querySelector('form [id=archiveSelector]'),
        folderSelector = document.querySelector('form [id=folderSelector]'),
        form = document.querySelector('form[id=archiveUpload]'),
        submit = form.querySelector('.submit'),
        addDescription = document.querySelector('.package-addDescription'),
        contactingServer = document.querySelector('.addPackage-contactingServer'),
        packageName = document.querySelector('.packageName'),
        projectName = document.querySelector('.projectName');

    let isArchiveInvalid = false;

    function handleFileSelection(e)
    {
        let matches;

        if (e.target === folderSelector && folderSelector.files.length) {
            let folder = folderSelector.files[0].webkitRelativePath.replace(/\\/g, '/').split('/')[0];
            addDescription.innerHTML = `Selected : folder <b>${folder}</b> with ${folderSelector.files.length} files`;
            archiveSelector.value = null;
            matches = null;
        } else if (e.target === archiveSelector && archiveSelector.files.length) {
            addDescription.innerHTML = `Selected : archive <b>${archiveSelector.files[0].name}</b>.`;
            folderSelector.value = null;
            matches = archiveSelector.files[0].name.match(/(.*?)[.](.*)/)
        } else
            addDescription.innerHTML = addDescription.getAttribute('data-original');

        isArchiveInvalid = false;
        if (matches && matches.length > 2)
        {
            const ext = matches[2];
            const allowed = ['zip', 'tar.gz'];
            if (!allowed.includes(ext))
            {
                isArchiveInvalid = true;
                archiveSelector.value = null;
                addDescription.innerHTML = addDescription.getAttribute('data-original');
                alert(`${ext} is not a support archive type. Only ${allowed.join(', ')} are allowed.` );
            }
        }

    }
    
    archiveSelector.addEventListener('input', handleFileSelection, false);
    folderSelector.addEventListener('input', handleFileSelection, false);
    

    addDescription.innerHTML = addDescription.getAttribute('data-original');

    submit.addEventListener('click', async function () {

        if (!packageName.value)
            return alert('Package name is required.');

        if (isArchiveInvalid)
            return alert('Archive type is invalid.');

        if (!folderSelector.files.length && !archiveSelector.files.length)
            return alert('Please select either an archive or a folder to upload.');

        let isArchive,
            removeFirstDirectoryFromPath;

        let formData = new FormData();

        if (folderSelector.files.length) {
            for (const item of folderSelector.files)
                formData.append('Files', item);

            isArchive = false;
            removeFirstDirectoryFromPath = true;
        } else if (archiveSelector.files.length) {
            formData.append('Files', archiveSelector.files[0]);
            isArchive = true;
            removeFirstDirectoryFromPath = false;
        } 
       
        const 
            package = encodeURIComponent(packageName.value),
            project = encodeURIComponent(projectName.value);

        submit.classList.add('button--disabled');
        contactingServer.classList.add('addPackage-contactingServer--show');

        try {
            const response = await fetch(`/v1/packages/${project}/${package}?isArchive=${isArchive}&removeFirstDirectoryFromPath=${removeFirstDirectoryFromPath}`, {
                method: 'POST',
                headers: {

                },
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