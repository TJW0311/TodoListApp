function openEditPopup(id, name) {
    document.getElementById("editProjectId").value = id;
    document.getElementById("editProjectName").value = name;
    document.getElementById("charCount").textContent = `${name.length}/25`;
    document.getElementById("nameError").classList.add("d-none");
    new bootstrap.Modal(document.getElementById("editProjectModal")).show();
}

function saveProjectName() {
    const id = document.getElementById("editProjectId").value;
    const name = document.getElementById("editProjectName").value.trim();

    if (!name || name.length > 25) {
        showToast("⚠ Project name cannot be empty or exceed 25 characters.");
        return;
    }

    fetch(`/Project/UpdateProjectName`, {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id, newName: name })
    })
        .then(res => {
            if (!res.ok) return res.text().then(msg => { throw new Error(msg); });
            return res.text();
        })
        .then(() => location.reload())
        .catch(err => {
            showToast(`❌ ${err.message}`);
        });
}

function openDeletePopup(id, name) {
    document.getElementById("deleteProjectId").value = id;
    document.getElementById("deleteProjectName").innerText = name;
    new bootstrap.Modal(document.getElementById("deleteProjectModal")).show();
}

function confirmDeleteProject() {
    const id = document.getElementById("deleteProjectId").value;

    fetch(`/Project/DeleteProject`, {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id })
    })
        .then(res => {
            if (!res.ok) return res.text().then(msg => { throw new Error(msg); });
            return res.text();
        })
        .then(() => location.reload())
        .catch(err => {
            showToast(`❌ ${err.message}`);
        });
}

document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("editProjectName");
    if (input) {
        input.addEventListener("input", function () {
            document.getElementById("charCount").textContent = `${this.value.length}/25`;
            document.getElementById("nameError").classList.add("d-none");
        });
    }
});

/*Real-time project name digit count*/
document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("editProjectName");
    const counter = document.getElementById("charCount");

    input.addEventListener("input", function () {
        const len = input.value.length;
        counter.textContent = `${len}/25`;

        if (len >= 25) {
            counter.classList.remove("text-muted");
            counter.classList.add("text-danger");
        } else {
            counter.classList.remove("text-danger");
            counter.classList.add("text-muted");
        }
    });
});