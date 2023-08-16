from pathlib import Path
from typing import List, Set
from github import Auth, Github
from github.Repository import Repository
from github.GithubException import GithubException

import os
import sys
import yaml


def add_reviewers(
    reviewers: Set[str], team_reviewers: Set[str], new_entries: List[str]
):
    for reviewer in new_entries:
        if reviewer.startswith("@"):
            team_reviewers.add(reviewer[1:])
        else:
            reviewers.add(reviewer)


def update_reviewers(config, repo: Repository, pr_id: int) -> int:
    pull_request = repo.get_pull(pr_id)

    if not pull_request:
        sys.stderr.writable(f"Unknown PR #{pr_id}\n")
        return 1

    if pull_request.draft:
        print("Not assigning reviewers for draft PRs")
        return 0

    pull_request_author = pull_request.user.login
    reviewers = set()
    team_reviewers = set()

    for label in pull_request.labels:
        if label.name in config:
            add_reviewers(reviewers, team_reviewers, config[label.name])

    if "default" in config:
        add_reviewers(reviewers, team_reviewers, config["default"])

    if pull_request_author in reviewers:
        reviewers.remove(pull_request_author)

    try:
        reviewers = list(reviewers)
        team_reviewers = list(team_reviewers)
        print(
            f"Attempting to assign reviewers ({reviewers}) and team_reviewers ({team_reviewers})"
        )
        pull_request.create_review_request(reviewers, team_reviewers)
        return 0
    except GithubException as e:
        sys.stderr.write(f"Cannot assign review request for PR #{pr_id}: {e}\n")
        return 1


if __name__ == "__main__":
    if len(sys.argv) != 7:
        sys.stderr.write("usage: <app_id> <private_key_env_name> <installation_id> <repo_path> <pr_id> <config_path>\n")
        sys.exit(1)

    app_id = sys.argv[1]
    private_key = os.environ[sys.argv[2]]
    installation_id = sys.argv[3]
    repo_path = sys.argv[4]
    pr_id = int(sys.argv[5])
    config_path = Path(sys.argv[6])

    auth = Auth.AppAuth(app_id, private_key).get_installation_auth(installation_id)
    g = Github(auth=auth)
    repo = g.get_repo(repo_path)

    if not repo:
        sys.stderr.write("Repository not found!\n")
        sys.exit(1)

    if not config_path.exists():
        sys.stderr.write(f'Config "{config_path}" not found!\n')
        sys.exit(1)

    with open(config_path, "r") as f:
        config = yaml.safe_load(f)

    sys.exit(update_reviewers(config, repo, pr_id))
