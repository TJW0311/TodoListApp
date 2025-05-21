const editPriorityDropdown = document.getElementById("edit-priority");
const editStatusDropdown = document.getElementById("edit-status");
const statusNotice = document.createElement("div");
statusNotice.id = "status-edit-notice";
statusNotice.className = "form-text text-warning mt-1";
statusNotice.style.display = "none";
editStatusDropdown.parentNode.appendChild(statusNotice);

const originalStatusOptions = Array.from(editStatusDropdown.options);

const STATUS = {
    Draft: "1",
    WaitingApproval: "2",
    Approved: "3",
    InProgress: "4",
    Completed: "5"
};

const userRole = document.getElementById("userRole").dataset.role;

function updateEditStatusDropdown(priorityValue, currentStatusValue) {
    priorityValue = String(priorityValue);
    currentStatusValue = String(currentStatusValue);

    editStatusDropdown.innerHTML = "";
    statusNotice.style.display = "none";
    statusNotice.textContent = "";

    let allowedStatusIds = [];

    if (priorityValue === "3") { // High priority
        if (currentStatusValue === STATUS.WaitingApproval) {
            if (userRole === "Manager") {
                allowedStatusIds = [STATUS.WaitingApproval, STATUS.Approved];
                editStatusDropdown.setAttribute("readonly", false);
                editStatusDropdown.classList.remove("readonly-select");
            } else {
                allowedStatusIds = [STATUS.WaitingApproval];
                editStatusDropdown.setAttribute("readonly", true);
                editStatusDropdown.classList.add("readonly-select");
                statusNotice.textContent = "⚠ Waiting for the manager approve";
                statusNotice.style.display = "block";
            }
        } else if ([STATUS.Approved, STATUS.InProgress, STATUS.Completed].includes(currentStatusValue)) {
            if (userRole === "Manager") {
                allowedStatusIds = [STATUS.Approved, STATUS.InProgress, STATUS.Completed];
            } else {
                allowedStatusIds = [STATUS.Approved, STATUS.InProgress, STATUS.Completed];
            }
            editStatusDropdown.setAttribute("readonly", false);
            editStatusDropdown.classList.remove("readonly-select");
        } else {
            // fallback
            allowedStatusIds = [STATUS.WaitingApproval];
            editStatusDropdown.setAttribute("readonly", true);
            editStatusDropdown.classList.add("readonly-select");
            statusNotice.textContent = "⚠ High priority requires approval by manager";
            statusNotice.style.display = "block";
        }
    } else if (priorityValue === "1" || priorityValue === "2") { // Low or Medium
        allowedStatusIds = [STATUS.Draft, STATUS.InProgress, STATUS.Completed];
        editStatusDropdown.setAttribute("readonly", false);
        editStatusDropdown.classList.remove("readonly-select");
    }

    originalStatusOptions.forEach(opt => {
        if (allowedStatusIds.includes(opt.value)) {
            editStatusDropdown.appendChild(opt.cloneNode(true));
        }
    });

    if (priorityValue === "3" && !allowedStatusIds.includes(currentStatusValue)) {
        editStatusDropdown.value = STATUS.WaitingApproval;
    } else {
        editStatusDropdown.value = currentStatusValue;
    }
}

// Handle dynamic change
editPriorityDropdown.addEventListener("change", function () {
    var newPriority = this.value;
    var currentStatus = document.getElementById("edit-status").value
    updateEditStatusDropdown(newPriority, currentStatus);
});

/*// Call once on modal open
document.getElementById("editModal").addEventListener("show.bs.modal", function () {
    var priority = editPriorityDropdown.value;
    var status = editStatusDropdown.value;
    
});*/

/*const saveButton = document.getElementById("save-edit");
const statusSelect = document.getElementById("edit-status");

saveButton.addEventListener("click", function () {
    console.log("Selected Status ID:", statusSelect.value);
    console.log("Selected Status Text:", statusSelect.options[statusSelect.selectedIndex].text);
});*/