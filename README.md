# gitlabels

> Manage GitHub labels using declarative files.


## Usage

Sync labels from local file named `labels.json`

```
gitlabels.exe -r <username>/<repo_name> -t <github_token> -c labels.json
```

Sync labels from a file in the repository. File must be named `.github/labels.json`
and it must exist in default branch.

```
gitlabels.exe -r <username>/<repo_name> -t <github_token>
```


You can find sample configuration files here: https://github.com/jozefizso/git-label-packages/tree/master/packages


## License

Copyright (c) 2017 Jozef Izso, [MIT License](LICENSE)
