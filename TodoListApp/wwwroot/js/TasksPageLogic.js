﻿document.addEventListener("DOMContentLoaded", function () {
    const editForm = document.getElementById("editForm");

    editForm.addEventListener("submit", function (e) {
        const start = new Date(document.getElementById("edit-start").value);
        const due = new Date(document.getElementById("edit-due").value);

        if (start > due) {
            e.preventDefault();
            alert("❗ Start Date must be before or equal to Due Date.");
        }
    });
});

// set value to the input for popup edit
document.getElementById("task-table-container").addEventListener("click", function (e) {
    const row = e.target.closest(".task-row");
    if (row) {
        const taskId = row.dataset.id;

        fetch(`/Home/GetTaskDetails?id=${taskId}`)
            .then(response => response.json())
            .then(task => {
                if (task) {
                    document.getElementById("edit-id").value = task.id;
                    document.getElementById("edit-name").value = task.name;
                    document.getElementById("edit-description").value = task.description;
                    document.getElementById("edit-category").value = task.categoryId;
                    document.getElementById("edit-start").value = task.startDate;
                    document.getElementById("edit-due").value = task.dueDate;
                    document.getElementById("edit-priority").value = task.priorityId;
                    document.getElementById("edit-status").value = task.statusId;
                    document.getElementById("edit-assignto").value = task.assignedToUserId;

                    if (task.statusId === 6) {
                        document.querySelectorAll("#editModal .modal-body input, #editModal .modal-body select").forEach(input => {
                            input.disabled = true;
                        });
                        document.getElementById("save-edit").style.display = "none";
                    } else {
                        updateEditStatusDropdown(task.priorityId, task.statusId);
                        document.querySelectorAll("#editModal .modal-body input, #editModal .modal-body select").forEach(input => {
                            input.disabled = false;
                        });
                        document.getElementById("save-edit").style.display = "inline-block";
                    }

                    document.getElementById("delete-id").value = task.id;
                    clearAllValidation(fields);
                }
            });
    }
});

const form = document.getElementById("editForm");
const saveBtn = document.querySelector('[form="editForm"]');
const fields = {
    name: document.getElementById("edit-name"),
    description: document.getElementById("edit-description"),
    category: document.getElementById("edit-category"),
    //start: document.getElementById("edit-start"),
    due: document.getElementById("edit-due"),
    priority: document.getElementById("edit-priority"),
    status: document.getElementById("edit-status"),
    assignee: document.getElementById("edit-assignto")
};

function validateField(field, condition, message) {
    const parent = field.parentElement;
    const existing = parent.querySelector(".text-danger");

    if (!condition) {
        field.classList.add("is-invalid");
        if (!existing) {
            const error = document.createElement("div");
            error.className = "text-danger small mt-1";
            error.innerText = message;
            parent.appendChild(error);
        }
        return false;
    } else {
        field.classList.remove("is-invalid");
        if (existing) existing.remove();
        return true;
    }
}

function validateAllFields() {
    let valid = true;

    valid &= validateField(fields.name, fields.name.value.trim() !== "", "Please enter task name.");
    valid &= validateField(fields.description, fields.description.value.trim() !== "", "Please enter description.");
    valid &= validateField(fields.category, fields.category.value, "Please select a category.");
    valid &= validateField(fields.start, fields.start.value, "Please select a start date.");
    valid &= validateField(fields.due, fields.due.value, "Please select a due date.");
    valid &= validateField(fields.priority, fields.priority.value, "Please select priority.");
    valid &= validateField(fields.status, fields.status.value, "Please select status.");
    valid &= validateField(fields.assignee, fields.assignee.value, "Please select a user.");

    //Check if start earlier than today
    if (fields.start.value) {
        todayDate = new Date();
        const startDate = new Date(fields.start.value);
        console.log(todayDate);
        console.log(startDate);
        console.log(startDate < todayDate);
        valid &= validateField(fields.start, startDate > todayDate , "Start date cannot be earlier than today.");
    }
    // Check if due >= start
    if (fields.start.value && fields.due.value) {
        const startDate = new Date(fields.start.value);
        const dueDate = new Date(fields.due.value);
        valid &= validateField(fields.due, dueDate >= startDate, "Due date cannot be earlier than start date.");
    }

    return !!valid;
}

function clearAllValidation(fields) {
    Object.values(fields).forEach(field => {
        const parent = field.parentElement;
        const error = parent.querySelector(".text-danger");

        field.classList.remove("is-invalid");
        if (error) error.remove();
    });
}

saveBtn.addEventListener("click", function (e) {
    if (!validateAllFields()) {
        e.preventDefault(); // stop form if invalid
    }
});

// Real-time validation
Object.values(fields).forEach(field => {
    field.addEventListener("input", () => validateAllFields());
    field.addEventListener("change", () => validateAllFields());
});

// Mark that a sort was clicked
document.querySelectorAll('.sort-link').forEach(link => {
    link.addEventListener('click', () => {
        sessionStorage.setItem('showSortToast', 'true');
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const searchType = document.getElementById("searchType");
    const searchValue = document.getElementById("searchValue");

    function updateInputType() {
        const selected = searchType.value;
        if (selected === "name") {
            searchValue.type = "text";
            searchValue.placeholder = "Enter name";
        } else {
            searchValue.type = "date";
            searchValue.placeholder = "";
        }
    }

    searchType.addEventListener("change", updateInputType);
    updateInputType(); // run on page load
});

document.addEventListener("DOMContentLoaded", function () {
    const searchType = document.getElementById("searchType");
    const searchValue = document.getElementById("searchValue");

    function updateInputType() {
        const selected = searchType.value;
        if (selected === "name") {
            searchValue.type = "text";
            searchValue.placeholder = "Enter name";
        } else {
            searchValue.type = "date";
            searchValue.placeholder = "";
        }
    }

    searchType.addEventListener("change", updateInputType);
    updateInputType(); // Run on page load
});

//Pagination
document.addEventListener("click", function (e) {
    if (e.target.classList.contains("pagination-link")) {
        e.preventDefault();

        const page = e.target.getAttribute("data-page");
        const id = e.target.getAttribute("data-id");
        const sortBy = e.target.getAttribute("data-sortby");
        const sortDir = e.target.getAttribute("data-sortdir");
        const searchType = e.target.getAttribute("data-searchtype");
        const searchValue = e.target.getAttribute("data-searchvalue");

        const urlParams = new URLSearchParams({
            page,
            id,
            sortBy,
            sortDir,
            searchType,
            searchValue
        });

        fetch(`/Home/Index?${urlParams.toString()}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(response => response.text())
            .then(html => {
                document.getElementById("task-table-container").innerHTML = html;
            });
    }
});

function openManageMembers(projectId) {
    $.get(`/Home/ManageMembersPartial?projectId=${projectId}`, function (html) {
        $("#manageMembersContent").html(html);
        $("#manageMemberModal").modal("show");
    });
}

function showToast(message) {
    const toast = $('<div class="toast-message">' + message + '</div>');
    $("body").append(toast);
    toast.fadeIn(300).delay(2000).fadeOut(500, function () {
        $(this).remove();
    });
}

function exportProjectTasks(projectId) {
    // First check if tasks exist
    fetch(`/Home/CheckTasksExist?projectId=${projectId}`)
        .then(res => res.json())
        .then(result => {
            if (!result.exists) {
                showToast("No tasks found to export.");
                return;
            }

            // Proceed to show modal and export
            showExportModalAndStart(projectId);
        })
        .catch(() => {
            showToast("Failed to check task availability.");
        });
}

let exportAbortController = null; // global reference

function showExportModalAndStart(projectId) {
    const modal = new bootstrap.Modal(document.getElementById("exportProgressModal"));
    const progressBar = document.getElementById("exportProgressBar");
    const cancelBtn = document.getElementById("cancelExportBtn");

    let percent = 0;
    progressBar.style.width = "0%";
    progressBar.textContent = "0%";
    modal.show();

    // Simulate fake progress
    const interval = setInterval(() => {
        if (percent < 95) {
            percent += Math.floor(Math.random() * 4) + 1;
            progressBar.style.width = percent + "%";
            progressBar.textContent = percent + "%";
        } else {
            clearInterval(interval);
            setTimeout(() => modal.hide(), 800);
        }
    }, 200);

    exportAbortController = new AbortController();

    fetch(`/Home/ExportTasksToExcel`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `projectId=${projectId}`,
        signal: exportAbortController.signal
    })
        .then(async response => {
            clearInterval(interval);
            exportAbortController = null;

            const contentType = response.headers.get("Content-Type");

            if (contentType.includes("application/json")) {
                const result = await response.json();
                modal.hide();
                showToast(result.message || "Export failed.");
                return;
            }

            progressBar.style.width = "100%";
            progressBar.textContent = "100%";

            const blob = await response.blob();
            const url = URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = `Project_${projectId}_Tasks_${Date.now()}.xlsx`;
            document.body.appendChild(a);
            a.click();
            a.remove();

            setTimeout(() => modal.hide(), 500);
        })
        .catch(err => {
            clearInterval(interval);
            modal.hide();
            if (err.name === 'AbortError') {
                showToast("Export cancelled by user.");
            } else {
                showToast("Something went wrong during export.");
            }
        });

    // Cancel button logic
    cancelBtn.onclick = function () {
        if (exportAbortController) {
            exportAbortController.abort(); // abort fetch
            exportAbortController = null;
        }
        clearInterval(interval);
        modal.hide();
    };
}

// Reset Modal When It Closes
document.getElementById("exportProgressModal").addEventListener("hidden.bs.modal", function () {
    const progressBar = document.getElementById("exportProgressBar");
    progressBar.style.width = "0%";
    progressBar.textContent = "0%";
});