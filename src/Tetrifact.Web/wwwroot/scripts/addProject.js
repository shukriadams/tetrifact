(async function () {

    let form = document.querySelector('form[id=addProject]'),
        submit = form.querySelector('.submit'),
        projectName = document.querySelector('.projectName');

    submit.addEventListener('click', async function () {
        const formData = new FormData(form),
            project = encodeURIComponent(projectName.value);

        if (!project)
            return alert('Project name is required.');

        try {
            const response = await fetch(`/v1/projects/${project}`, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                window.location = `/projects/${project}`;
            } else {
                console.log(response);
            }

        } catch (err) {
            console.log(err);
            alert('Error:', err);
        }
    }, false);

})()